using Scripts.SkillSystem.Skills;
using Chipmunk.ComponentContainers;
using Code.ETC;
using Code.SHS.Entities.Enemies;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.FSM;
using UnityEngine;

namespace Scripts.SkillSystem.Skills.Grab
{
    public class GrabSkill : ActiveSkill,IUseStateSkill
    {
        [Header("Projectile")]
        [Tooltip("Optional projectile prefab. If empty, a runtime projectile object is created.")]
        [SerializeField]
        private GrabHookProjectile hookProjectilePrefab;

        [Tooltip("Projectile spawn transform. If empty, owner position is used.")] [SerializeField]
        private Transform firePoint;

        [Tooltip("Physics layers that the hook projectile can hit.")] [SerializeField]
        private LayerMask hitMask = ~0;

        [Tooltip("Projectile travel speed (units per second).")] [SerializeField]
        private float projectileSpeed = 35f;

        [Tooltip("Maximum travel distance before the projectile expires.")] [SerializeField]
        private float projectileRange = 20f;

        [Tooltip("Maximum lifetime in seconds before auto-destroy.")] [SerializeField]
        private float projectileLifeTime = 2f;

        [Tooltip("How far in front of the origin the projectile starts.")] [SerializeField]
        private float spawnForwardOffset = 0.6f;

        [Header("Pull")] [Tooltip("Movement profile applied while pulling the hit target.")] [SerializeField]
        private MovementDataSO pullMovementData;

        [Tooltip("Target is pulled toward this transform. If empty, owner transform is used.")] [SerializeField]
        private Transform pullAnchor;

        [Tooltip("Stop pulling when target is within this distance from the anchor.")] [SerializeField]
        private float pullStopDistance = 1.2f;

        [Tooltip("Control lock duration in seconds (stun). Final lock time is at least movement duration.")]
        [SerializeField]
        private float controlLockDuration = 1f;

        [Header("Damage")] [Tooltip("Base damage before multipliers and stats are applied.")] [SerializeField]
        private float defaultDamage = 8f;

        [Tooltip("Damage multiplier applied to base damage.")] [SerializeField]
        private float damageMultiplier = 1f;

        [Tooltip("Defense penetration level used by the damage calculator.")] [SerializeField]
        private int defPierceLevel;

        [Tooltip("Damage category used for resistances and reactions.")] [SerializeField]
        private DamageType damageType = DamageType.RANGE;

        private IAimProvider _aimProvider;
        private DamageCalcCompo _damageCalcCompo;
        private MovementDataSO _fallbackPullMovementData;

        public SkillAnimType AnimType => SkillAnimType.Grab;

        [field:SerializeField]public StateDataSO TargetState { get; private set; }

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            _damageCalcCompo = container.Get<DamageCalcCompo>();
        }

        private MovementDataSO GetPullMovementData()
        {
            if (pullMovementData != null)
                return pullMovementData;

            if (_fallbackPullMovementData == null)
            {
                _fallbackPullMovementData = ScriptableObject.CreateInstance<MovementDataSO>();
                _fallbackPullMovementData.maxSpeed = 22f;
                _fallbackPullMovementData.duration = 0.45f;
                _fallbackPullMovementData.moveCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
            }

            return _fallbackPullMovementData;
        }

        private DamageData BuildDamageData()
        {
            if (_damageCalcCompo != null)
            {
                return _damageCalcCompo.CalculateDamage(
                    defaultDamage,
                    damageMultiplier,
                    defPierceLevel,
                    damageType);
            }

            return new DamageData
            {
                damage = defaultDamage * damageMultiplier,
                defPierceLevel = defPierceLevel,
                damageType = damageType
            };
        }

        private Vector3 GetFireDirection(Vector3 origin)
        {
            Vector3 direction = _owner.transform.forward;

            if (_owner is Enemy enemy)
            {
                Entity target = enemy.TargetProvider.CurrentTarget;
                if (target != null)
                    direction = target.transform.position - origin;
            }
            else if (_aimProvider != null)
            {
                direction = _aimProvider.GetAimPosition() - origin;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                direction = _owner.transform.forward;

            return direction.normalized;
        }

        public void OnSkillTrigger()
        {
            MovementDataSO movementData = GetPullMovementData();
            Vector3 origin = firePoint != null ? firePoint.position : _owner.transform.position;
            Vector3 direction = GetFireDirection(origin);
            Vector3 spawnPos = origin + direction * spawnForwardOffset;
            Transform targetAnchor = pullAnchor != null ? pullAnchor : _owner.transform;
            DamageData damageData = BuildDamageData();

            GrabHookProjectile projectile;
            if (hookProjectilePrefab != null)
            {
                projectile = Instantiate(
                    hookProjectilePrefab,
                    spawnPos,
                    Quaternion.LookRotation(direction));
            }
            else
            {
                GameObject runtimeProjectile = new GameObject("GrabHookProjectile_Runtime");
                runtimeProjectile.transform.SetPositionAndRotation(spawnPos, Quaternion.LookRotation(direction));
                projectile = runtimeProjectile.AddComponent<GrabHookProjectile>();
            }

            projectile.HitMask = hitMask;
            projectile.Speed = projectileSpeed;
            projectile.MaxDistance = projectileRange;
            projectile.LifeTime = projectileLifeTime;
            projectile.PullMovementData = movementData;
            projectile.PullStopDistance = pullStopDistance;
            projectile.ControlLockDuration = controlLockDuration;
            projectile.Launch(_owner, targetAnchor, direction, damageData);
        }
    }
}
