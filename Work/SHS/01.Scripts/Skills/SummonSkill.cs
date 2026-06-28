using System;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Scripts.SkillSystem.Skills;
using Scripts.FSM;
using Scripts.SkillSystem;
using SHS.Scripts.Summon;
using SHS.Scripts.Summon.Turrets;
using UnityEngine;

namespace SHS.Scripts.Skills
{
    public class SummonSkill : ActiveSkill
    {
        [SerializeField] private SoundID turretThrowSound;
        [SerializeField] private TurretDummy summonPrefab;
        [SerializeField] private Transform summonTransform;
        
        [Header("Random Spawn Settings")]
        [SerializeField] private Transform spawnTrm;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float minSpawnRadius = 2f;  // 추가
        [SerializeField] private int maxRetryCount = 10;
        [SerializeField] private float overlapCheckRadius = 0.5f;
        [SerializeField] private LayerMask obstacleLayer;
        private EngineerTurretTracker _turretTracker;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            container.TryGetComponent(out _turretTracker);
        }

        private void OnValidate()
        {
            Debug.Assert(summonPrefab.GetComponent<ISummonable>() != null,
                "SummonPrefab does not implement ISummonable");
        }

        public override void OnSkillTrigger()
        {
            Summon();

        }
        private GameObject Summon()
        {
            Vector3 origin = transform.position;
            Quaternion rotation = summonTransform != null ? summonTransform.rotation : transform.rotation;

            if (!TryGetValidSpawnPoint(origin, out Vector3 targetPoint))
            {
                Debug.LogWarning("유효한 소환 위치를 찾지 못했습니다.");
                return null;
            }
            
            TurretDummy dummy = Instantiate(summonPrefab, spawnTrm.position, rotation);

            Debug.Log(targetPoint);
            if (_turretTracker != null)
            {
                _turretTracker.Register(dummy.gameObject);
                dummy.SetTracker(_turretTracker);
            }

            dummy.Throw(targetPoint);
            BroAudio.Play(turretThrowSound, _owner.transform.position);
            return dummy.gameObject;
        }

        private bool TryGetValidSpawnPoint(Vector3 origin, out Vector3 result)
        {
            for (int i = 0; i < maxRetryCount; i++)
            {
                Vector3 candidate = GetRandomPointInCircle(origin);

                if (!IsObstacleAt(candidate))
                {
                    result = candidate;
                    return true;
                }
            }

            result = Vector3.zero;
            return false;
        }

        private Vector3 GetRandomPointInCircle(Vector3 origin)
        {
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float distance = Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f)) * (spawnRadius - minSpawnRadius) + minSpawnRadius;

            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;

            return origin + new Vector3(x, 0f, z);
        }

        private bool IsObstacleAt(Vector3 point)
        {
            return Physics.CheckSphere(point, overlapCheckRadius, obstacleLayer);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = summonTransform != null ? summonTransform.position : transform.position;

            // 소환 가능 범위
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(origin, spawnRadius);

            // 장애물 체크 반경
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
            Gizmos.DrawWireSphere(origin, overlapCheckRadius);
        }
#endif
    }
}
