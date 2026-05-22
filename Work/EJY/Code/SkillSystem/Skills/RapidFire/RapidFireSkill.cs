using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Code.ETC;
using Cysharp.Threading.Tasks;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.SkillSystem;
using SHS.Scripts;
using UnityEngine;

namespace Code.SkillSystem.Skills.RapidFire
{
    public class RapidFireSkill : ActiveSkill, IProjectileShooter
    {
        [SerializeField] private Transform firePos;
        [SerializeField] private PoolItemSO bulletPoolItem;
        [SerializeField] private LayerMask excludeLayer;

        [SerializeField] private float damage = 3.5f;
        [SerializeField] private float projectileSpeed = 35f;
        [SerializeField] private float projectileMaxRange = 20f;

        [SerializeField] private float shotInterval = 0.08f;

        [SerializeField] private float sameTargetDamageBonus = 0.2f;

        [SerializeField] private int shotCount = 8;
        [SerializeField] private int extraShotCount = 4;

        [SerializeField] private int defPierceLevel = 1;

        [SerializeField] private float spreadAngle = 3f;

        [Inject]
        private PoolManagerMono _poolManager;

        private IAimProvider _aimProvider;

        private bool _additionalShotsUnlocked;
        private bool _sameTargetDamageUnlocked;

        private bool _isBurstRunning;

        private readonly Dictionary<Transform, int> _hitStacks
            = new();

        public float DefaultDamage => damage;

        public float ProjectileSpeed => projectileSpeed;

        public float ProjectileMaxRange => projectileMaxRange;

        public float DamageMultiplier => 1f;

        public int DefPierceLevel => defPierceLevel;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);

            _aimProvider =
                container.GetSubclassComponent<IAimProvider>();
        }

        public override bool CanUseSkill()
        {
            return base.CanUseSkill()
                   && _isBurstRunning == false
                   && bulletPoolItem != null
                   && _poolManager != null;
        }

        private void UpgradeAdditionalShots()
        {
            _additionalShotsUnlocked = true;
        }

        private void RollbackAdditionalShots()
        {
            _additionalShotsUnlocked = false;
        }

        private void UpgradeSameTargetDamage()
        {
            _sameTargetDamageUnlocked = true;
        }

        private void RollbackSameTargetDamage()
        {
            _sameTargetDamageUnlocked = false;
        }

        public override void StartAndUseSkill()
        {
            if (_isBurstRunning)
                return;

            FireBurstAsync().Forget();
        }

        private async UniTaskVoid FireBurstAsync()
        {
            _isBurstRunning = true;

            try
            {
                int totalShots =
                    shotCount +
                    (_additionalShotsUnlocked
                        ? extraShotCount
                        : 0);

                for (int i = 0; i < totalShots; i++)
                {
                    Vector3 aimPoint =
                        _aimProvider.GetAimPosition(
                            firePos.position.y);

                    _owner.RotateToTarget(aimPoint);

                    FireSingleShot(aimPoint);

                    if (i < totalShots - 1)
                    {
                        await UniTask.WaitForSeconds(
                            shotInterval);
                    }
                }
            }
            finally
            {
                _isBurstRunning = false;
            }
        }

        private void FireSingleShot(Vector3 aimPoint)
        {
            Transform origin =
                firePos != null
                    ? firePos
                    : transform;

            Vector3 direction =
                aimPoint - origin.position;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = _owner.transform.forward;
            }

            direction.Normalize();

            direction =
                ApplySpread(direction);

            RapidFireBullet bullet =
                _poolManager.Pop<RapidFireBullet>(
                    bulletPoolItem);

            bullet.SetSourceSkill(this);

            bullet.InitProjectile(
                _owner,
                this,
                origin.position,
                direction,
                excludeLayer);
        }

        private Vector3 ApplySpread(Vector3 direction)
        {
            float yaw =
                Random.Range(
                    -spreadAngle,
                    spreadAngle);

            float pitch =
                Random.Range(
                    -spreadAngle,
                    spreadAngle);

            Quaternion spreadRotation =
                Quaternion.Euler(
                    pitch,
                    yaw,
                    0f);

            return spreadRotation * direction;
        }

        public float RegisterHitAndGetMultiplier(
            Transform target)
        {
            if (_sameTargetDamageUnlocked == false)
                return 0f;

            if (target == null)
                return 0f;

            Transform targetRoot =
                target.root;

            if (_hitStacks.TryGetValue(
                    targetRoot,
                    out int hitCount))
            {
                hitCount++;

                _hitStacks[targetRoot] =
                    hitCount;
            }
            else
            {
                hitCount = 1;

                _hitStacks.Add(
                    targetRoot,
                    hitCount);
            }

            return hitCount
                   * sameTargetDamageBonus;
        }
    }
}
