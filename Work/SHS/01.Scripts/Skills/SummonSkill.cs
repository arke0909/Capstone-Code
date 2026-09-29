using System;
using Ami.BroAudio;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using DewmoLib.ObjectPool.RunTime;
using Entities;
using Scripts.Combat;
using System.Collections;
using Scripts.SkillSystem.Skills;
using Scripts.FSM;
using Scripts.SkillSystem;
using SHS.Scripts.Summon;
using SHS.Scripts.Summon.Turrets;
using UnityEngine;

namespace SHS.Scripts.Skills
{
    public class SummonSkill : ActiveSkill, IStateExitControlledSkill
    {
        private const string ShieldVfxName = "HolyShield";

        [SerializeField] private SoundID turretThrowSound;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO summonPoolItem;
        [SerializeField] private Transform summonTransform;

        [Header("Cast Settings")]
        [SerializeField, Min(0f)] private float throwInterval = 3f;
        [SerializeField, Min(1)] private int throwCount = 3;
        [SerializeField, Range(0f, 1f)] private float shieldHealthRatio = 0.15f;
        [SerializeField] private float shieldVfxLocalYOffset = 0.77f;

        [Header("Random Spawn Settings")]
        [SerializeField] private Transform spawnTrm;
        [SerializeField] private float spawnRadius = 5f;
        [SerializeField] private float minSpawnRadius = 2f;  // 추가
        [SerializeField] private int maxRetryCount = 10;
        [SerializeField] private float overlapCheckRadius = 0.5f;
        [SerializeField] private LayerMask obstacleLayer;

        private EngineerTurretTracker _turretTracker;
        private ShieldCompo _shieldCompo;
        private HealthCompo _healthCompo;
        private VFXComponent _vfxComponent;
        private ShieldInstance _castingShield;
        private Coroutine _throwRoutine;
        private bool _isCasting;
        private bool _isShieldVfxPlaying;

        public bool IsSkillComplete { get; private set; } = true;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            container.TryGetComponent(out _turretTracker);
            container.TryGetComponent(out _shieldCompo);
            container.TryGetComponent(out _healthCompo);
            container.TryGetComponent(out _vfxComponent);

            // Keep the caster stationary while the skill state's idle animation loops.
            MoveType = SkillMoveType.Stop;
        }

        private void OnValidate()
        {
            if (summonPoolItem == null || summonPoolItem.prefab == null)
                return;

            Debug.Assert(summonPoolItem.prefab.GetComponent<TurretDummy>() != null,
                $"{nameof(summonPoolItem)} prefab requires {nameof(TurretDummy)}.");
        }

        public override void StartSkill()
        {
            base.StartSkill();
            CleanupCast();

            IsSkillComplete = false;

            if (_shieldCompo == null || _healthCompo == null)
            {
                Debug.LogError($"{nameof(SummonSkill)} requires {nameof(ShieldCompo)} and {nameof(HealthCompo)}.", this);
                IsSkillComplete = true;
                return;
            }

            _isCasting = true;
            float shieldAmount = _healthCompo.MaxValue * shieldHealthRatio;
            _castingShield = _shieldCompo.AddShield(shieldAmount, HandleShieldBroken);
            PlayShieldVfx();
        }

        public override void OnSkillTrigger()
        {
            if (!_isCasting || _throwRoutine != null)
                return;

            _throwRoutine = StartCoroutine(ThrowTurrets());
        }

        public override void EndSkill()
        {
            base.EndSkill();
            CleanupCast();
            IsSkillComplete = true;
        }

        private IEnumerator ThrowTurrets()
        {
            for (int i = 0; i < throwCount; i++)
            {
                if (!_isCasting)
                    yield break;

                Summon();

                if (i < throwCount - 1)
                    yield return new WaitForSeconds(throwInterval);
            }

            _throwRoutine = null;
            _isCasting = false;
            IsSkillComplete = true;
        }

        private void HandleShieldBroken()
        {
            if (!_isCasting)
                return;

            _castingShield = null;
            _isCasting = false;
            StopShieldVfx();

            if (_throwRoutine != null)
            {
                StopCoroutine(_throwRoutine);
                _throwRoutine = null;
            }

            IsSkillComplete = true;
        }

        private void CleanupCast()
        {
            _isCasting = false;
            StopShieldVfx();

            if (_throwRoutine != null)
            {
                StopCoroutine(_throwRoutine);
                _throwRoutine = null;
            }

            if (_castingShield == null)
                return;

            _castingShield.OnBroken -= HandleShieldBroken;
            _shieldCompo?.RemoveShield(_castingShield);
            _castingShield = null;
        }

        private void PlayShieldVfx()
        {
            if (_vfxComponent == null)
            {
                Debug.LogWarning($"{nameof(SummonSkill)} could not find {nameof(VFXComponent)}.", this);
                return;
            }

            Vector3 localPosition = Vector3.up * shieldVfxLocalYOffset;
            _vfxComponent.PlayVFX(ShieldVfxName, localPosition, Quaternion.identity, true);
            _isShieldVfxPlaying = true;
        }

        private void StopShieldVfx()
        {
            if (!_isShieldVfxPlaying)
                return;

            _vfxComponent?.StopVFX(ShieldVfxName);
            _isShieldVfxPlaying = false;
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
            
            TurretDummy dummy = poolManager?.Pop(summonPoolItem) as TurretDummy;
            if (dummy == null)
            {
                Debug.LogError($"Failed to pop {nameof(TurretDummy)} from the pool.", this);
                return null;
            }

            dummy.transform.SetPositionAndRotation(spawnTrm.position, rotation);

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
