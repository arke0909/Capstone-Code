using System;
using Chipmunk.ComponentContainers;
using Code.ETC;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Skills;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SHS.Scripts.Skills.Miner
{
    public class CaveInSkill : ActiveSkill
    {
        [Header("Required")]
        [SerializeField] private PoolItemSO rockItem;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask spawnBlockLayer;

        [Header("Spawn")]
        [SerializeField] private int rockCount = 4;
        [SerializeField] private int maxSpawnRetryCount = 24;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float minSpawnRadius = 1f;
        [SerializeField] private float spawnCheckRadius = 0.7f;
        [SerializeField] private float groundRayHeight = 12f;
        [SerializeField] private float groundRayDistance = 30f;

        [Inject] private PoolManagerMono _poolManager;

        private IAimProvider _aimProvider;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);

            if (rockItem == null)
                throw new InvalidOperationException($"{nameof(CaveInSkill)} requires {nameof(rockItem)}.");

            if (groundLayer.value == 0)
                throw new InvalidOperationException($"{nameof(CaveInSkill)} requires {nameof(groundLayer)}.");

            if (spawnBlockLayer.value == 0)
                throw new InvalidOperationException($"{nameof(CaveInSkill)} requires {nameof(spawnBlockLayer)}.");

            _aimProvider = container.GetSubclassComponent<IAimProvider>();

            if (_aimProvider == null)
                throw new InvalidOperationException($"{nameof(CaveInSkill)} requires {nameof(IAimProvider)}.");
        }

        public override void OnSkillTrigger()
        {
            if (_poolManager == null)
                throw new InvalidOperationException($"{nameof(CaveInSkill)} requires {nameof(PoolManagerMono)} injection.");

            Vector3 center = _aimProvider.GetWorldAimPosition();
            int spawnedCount = 0;

            for (int i = 0; i < maxSpawnRetryCount && spawnedCount < rockCount; i++)
            {
                Vector3 candidate = center + GetRandomSpawnOffset();
                Vector3 rayOrigin = candidate + Vector3.up * groundRayHeight;

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundRayDistance, groundLayer))
                    continue;

                if (Physics.CheckSphere(hit.point, spawnCheckRadius, spawnBlockLayer, QueryTriggerInteraction.Ignore))
                    continue;

                CaveInRock caveInRock = _poolManager.Pop<CaveInRock>(rockItem);

                if (caveInRock == null)
                    throw new InvalidOperationException($"{nameof(CaveInSkill)} failed to pop {nameof(CaveInRock)} from pool.");

                caveInRock.Init(_owner, hit.point);
                spawnedCount++;
            }

            if (spawnedCount < rockCount)
                Debug.LogWarning($"{nameof(CaveInSkill)} spawned {spawnedCount}/{rockCount} rocks.", this);
        }

        private Vector3 GetRandomSpawnOffset()
        {
            float angleRadians = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minSpawnRadius, spawnRadius);

            return new Vector3(
                Mathf.Cos(angleRadians) * distance,
                0f,
                Mathf.Sin(angleRadians) * distance);
        }

        private void Reset()
        {
            AnimType = SkillAnimType.CaveIn;
        }

        private void OnValidate()
        {
            AnimType = SkillAnimType.CaveIn;
            Debug.Assert(rockItem != null, $"{nameof(CaveInSkill)} requires {nameof(rockItem)}.", this);
            Debug.Assert(groundLayer.value != 0, $"{nameof(CaveInSkill)} requires {nameof(groundLayer)}.", this);
            Debug.Assert(spawnBlockLayer.value != 0, $"{nameof(CaveInSkill)} requires {nameof(spawnBlockLayer)}.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.2f, 0.05f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, spawnRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minSpawnRadius);
        }
    }
}
