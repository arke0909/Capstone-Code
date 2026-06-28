using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.ETC;
using Code.Items;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Effects;
using Scripts.Entities;
using Scripts.Players;
using SHS.Scripts.Combats.Events;
using SHS.Scripts.Entities.Players;
using SHS.Scripts.NoiseSystems;
using UnityEngine;

namespace Scripts.Combat.ItemObjects
{
    public class GunObject : WeaponObject
    {
        private const float ConvergenceBlendDistance = 0.75f;

        [SerializeField] private NoiseGenerator _noiseGenerator;
        [SerializeField] private Transform fireTrm;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO bulletItem;
        [SerializeField] private PoolItemSO muzzleEffectItem;
        [SerializeField] private ParticleSystem shellEjectEffect;
        //[SerializeField] private float aimRotateSpeed = 28f;

        protected GunItem _gunItem => _item as GunItem;
        private GunDataSO _gunData;
        private IAimProvider _aimProvider;
        private LocalEventBus _localEventBus;
        private PlayerGunShotAssist _playerGunShotAssist;
        private float _currentSpread;
        private float _adsProgress;

        public float CurrentSpreadAngleDeg => Mathf.Clamp(_currentSpread * CurrentSpreadData.spreadFactor, 0f, 89f);
        public Vector3 FirePosition => fireTrm.position;

        public Vector3 FireDirection => GetBaseFireDirection();

        public Transform FireTrm => fireTrm;

        private GunSpreadData CurrentSpreadData => new GunSpreadData
        {
            defaultSpread = Mathf.Lerp(_gunData.hipFire.defaultSpread, _gunData.ads.defaultSpread, _adsProgress),
            maxSpread = Mathf.Lerp(_gunData.hipFire.maxSpread, _gunData.ads.maxSpread, _adsProgress),
            spreadGrow = Mathf.Lerp(_gunData.hipFire.spreadGrow, _gunData.ads.spreadGrow, _adsProgress),
            spreadRecover = Mathf.Lerp(_gunData.hipFire.spreadRecover, _gunData.ads.spreadRecover, _adsProgress),
            spreadFactor = Mathf.Lerp(_gunData.hipFire.spreadFactor, _gunData.ads.spreadFactor, _adsProgress)
        };

        public override void InitObject(Entity owner, EquipableItem item)
        {
            base.InitObject(owner, item);
            Debug.Assert(item is GunItem, "Invalid ItemType");
            Debug.Assert(_noiseGenerator != null, "NoiseGenerator is not assigned!");
            Debug.Assert(fireTrm != null, "FireTrm is not assigned!");
            Debug.Assert(poolManager != null, "PoolManager is not assigned!");
            Debug.Assert(bulletItem != null, "BulletItem is not assigned!");
            Debug.Assert(muzzleEffectItem != null, "MuzzleEffectItem is not assigned!");
            _gunData = _gunItem.GunItemData;
            Debug.Assert(_gunData.adsTime > 0f, "AdsTime must be greater than zero!");
            _aimProvider = owner.GetSubclassCompo<IAimProvider>();
            _playerGunShotAssist = null;
            if (owner is Player)
            {
                _playerGunShotAssist = owner.Get<PlayerGunShotAssist>();
                Debug.Assert(_playerGunShotAssist != null, "PlayerGunShotAssist is not assigned.");
            }
            _currentSpread = _gunData.hipFire.defaultSpread;
            _adsProgress = 0f;
            _localEventBus = owner.Get<LocalEventBus>();
        }

        public override void Attack()
        {
            Vector3 planeAimPoint = _aimProvider.GetAimPosition(fireTrm.position.y);
            Vector3 baseDirection = GetBaseFireDirection();
            GunSpreadData spreadData = CurrentSpreadData;
            float spreadAngleDeg = CurrentSpreadAngleDeg;

            for (int i = 0; i < _gunData.bulletPerShot; i++)
            {
                Vector3 direction = ApplySpreadCone(baseDirection, spreadAngleDeg);

                Bullet proj = poolManager.Pop(bulletItem) as Bullet;
                Debug.Assert(proj != null, $"Projectile Pool is empty : Pool Item ({bulletItem.name})");
                proj.InitProjectile(_owner, _gunItem, fireTrm.position, direction, 1 << _owner.gameObject.layer);

                if (_playerGunShotAssist != null &&
                    _playerGunShotAssist.TryGetImmediateHit(fireTrm.position, direction,
                        out Collider hitCollider, out Vector3 hitPoint, out Vector3 hitNormal))
                    proj.HitImmediately(hitCollider, hitPoint, hitNormal);
            }

            PoolingEffect muzzleEffect = poolManager.Pop(muzzleEffectItem) as PoolingEffect;
            Debug.Assert(muzzleEffect != null, $"MuzzleEffect Pool is empty : Pool Item ({muzzleEffectItem.name})");
            muzzleEffect.PlayVFX(fireTrm.position, Quaternion.LookRotation(baseDirection));

            float verticalRecoil = _gunData.verticalRecoil
                                   * Random.Range(_gunData.minVerticalMultiplier,
                                       _gunData.maxVerticalMultiplier);
            float horizontalRecoilRange =
                (Mathf.Abs(_gunData.minHorizontalMultiplier) + Mathf.Abs(_gunData.maxHorizontalMultiplier)) * 0.5f;
            float horizontalRecoil = _gunData.horizontalRecoil
                                     * Random.Range(-horizontalRecoilRange, horizontalRecoilRange);

            _localEventBus.Raise(new GunAttackEvent(_gunData, planeAimPoint, verticalRecoil, horizontalRecoil));

            _currentSpread = Mathf.Min(_currentSpread + spreadData.spreadGrow, spreadData.maxSpread);

            _noiseGenerator.GenerateNoise(_owner, _gunData.noiseRadius);
            if (shellEjectEffect != null)
                shellEjectEffect.Play();
        }

        private void Update()
        {
            if (_gunItem.IsAiming)
                _adsProgress = Mathf.MoveTowards(_adsProgress, 1f, Time.deltaTime / _gunData.adsTime);
            else
                _adsProgress = 0f;

            GunSpreadData spreadData = CurrentSpreadData;
            _currentSpread = Mathf.Clamp(_currentSpread, spreadData.defaultSpread, spreadData.maxSpread);
            _currentSpread = Mathf.MoveTowards(_currentSpread, spreadData.defaultSpread,
                spreadData.spreadRecover * Time.deltaTime);
        }

        private Vector3 GetBaseFireDirection()
        {
            Vector3 aimPoint = _aimProvider.GetAimPosition(fireTrm.position.y);

            Vector3 forward = _owner.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 directDirection = aimPoint - fireTrm.position;
            directDirection.y = 0f;

            float forwardDistance = Vector3.Dot(directDirection, forward);
            if (forwardDistance <= 0f)
                return forward;

            directDirection.Normalize();

            float blend = Mathf.InverseLerp(0f, ConvergenceBlendDistance, forwardDistance);
            return Vector3.Slerp(forward, directDirection, Mathf.SmoothStep(0f, 1f, blend)).normalized;
        }

        private static Vector3 ApplySpreadCone(Vector3 forward, float spreadAngleDeg)
        {
            if (spreadAngleDeg <= 0f) return forward;

            float yawDelta = Mathf.Lerp(-spreadAngleDeg, spreadAngleDeg, Random.value);
            Quaternion yawRot = Quaternion.AngleAxis(yawDelta, Vector3.up);
            return (yawRot * forward).normalized;
        }

        private void OnDrawGizmos()
        {
            if (_aimProvider == null || fireTrm == null)
                return;

            Vector3 aimPoint = _aimProvider.GetAimPosition(fireTrm.position.y);
            Gizmos.DrawLine(fireTrm.position, aimPoint);
            Gizmos.DrawWireSphere(aimPoint, 0.5f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(fireTrm.position, GetBaseFireDirection() * 3f);
        }
    }
}
