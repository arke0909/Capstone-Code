using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.ETC;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Combat.Fovs;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using SHS.Scripts.Combats.Events;
using SHS.Scripts.NoiseSystems;

namespace Scripts.Combat.ItemObjects
{
    public class GunObject : WeaponObject
    {
        [SerializeField] private NoiseGenerator _noiseGenerator;
        [SerializeField] private Transform fireTrm;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private PoolItemSO bulletItem;
        [SerializeField] private ParticleSystem shellEjectEffect;
        //[SerializeField] private float aimRotateSpeed = 28f;

        protected GunItem _gunItem => _item as GunItem;
        private GunDataSO _gunData;
        private IAimProvider _aimProvider;
        private LocalEventBus _localEventBus;
        private float _currentSpread;
        private float _lastShootTime;

        public float CurrentSpreadAngleDeg => GetCurrentAdsSpreadAngleDeg();
        public Vector3 FirePosition => fireTrm != null ? fireTrm.position : transform.position;

        public Vector3 FireDirection =>
            fireTrm != null ? _aimProvider.GetAimPosition() - fireTrm.position : Vector3.zero;

        public Transform FireTrm => fireTrm;

        public override void InitObject(Entity owner, EquipableItem item)
        {
            base.InitObject(owner, item);
            Debug.Assert(item is GunItem, "Invalid ItemType");
            Debug.Assert(_noiseGenerator != null, "NoiseGenerator is not assigned!");
            _gunData = _gunItem.GunItemData;
            _aimProvider = owner.GetSubclassCompo<IAimProvider>();
            _currentSpread = _gunData.defaultSpread;
            _localEventBus = owner.Get<LocalEventBus>();
        }

        public override void Attack()
        {
            Vector3 aimPoint = _aimProvider.GetAimPosition();

            for (int i = 0; i < _gunData.bulletPerShot; i++)
            {
                float spreadValue = GetCurrentAdsSpreadAngleDeg();
                Vector3 direction = aimPoint - fireTrm.position;
                direction.y = 0f;
                direction.Normalize();
                direction = ApplySpreadCone(direction, spreadValue);

                Bullet proj = poolManager.Pop(bulletItem) as Bullet;
                proj.InitBullet(_owner, _gunItem, fireTrm.position, direction);
            }

            _localEventBus.Raise(new GunAttackEvent(_gunData, GetCurrentAdsSpreadAngleDeg(), aimPoint));

            _currentSpread = Mathf.Min(_currentSpread + _gunData.spreadGrow, _gunData.maxSpread);
            _lastShootTime = Time.time;

            _noiseGenerator.GenerateNoise(_owner, _gunData.noiseRadius);
            shellEjectEffect?.Play();
        }

        private void Update()
        {
            RecoverScatterADS();
        }

        private void RecoverScatterADS()
        {
            // if (_currentSpread <= _gunData.defaultSpread) return;
            _currentSpread = Mathf.MoveTowards(_currentSpread, _gunData.defaultSpread,
                _gunData.spreadRecover * Time.deltaTime);
        }

        private float GetCurrentAdsSpreadAngleDeg()
        {
            return _currentSpread * _gunData.spreadFactor;
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

            Vector3 aimPoint = _aimProvider.GetAimPosition();
            Gizmos.DrawLine(fireTrm.position, aimPoint);
            Gizmos.DrawWireSphere(aimPoint, 0.5f);
        }
    }
}