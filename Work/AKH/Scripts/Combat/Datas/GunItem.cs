using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Entities;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using Code.Combat;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;
using SHS.Scripts;
using Scripts.Combat.ItemObjects;

namespace Scripts.Combat.Datas
{
    public class GunItem : Weapon, IReloadable, IProjectileShooter
    {
        public GunObject GunObj => WeaponObj as GunObject;
        public BulletItem currentBulletItem;
        public GunDataSO GunItemData => EquipItemData as GunDataSO;

        public int CurrentBulletCnt => _currentBullet;
        public bool IsAiming { get; private set; }
        public float DefaultDamage => GunItemData.defaultDamage;
        public float ProjectileSpeed => GunItemData.bulletSpeed;
        public float ProjectileMaxRange => GunItemData.maxRange > 0f ? GunItemData.maxRange : GunItemData.attackRange;
        public float DamageMultiplier => BulletData.damageMultiplier;
        public int DefPierceLevel => BulletData.defPierceLevel;
        public BulletDataSO BulletData => currentBulletItem.bulletDataSO;

        private float FireInterval => GunItemData.fireRate * _entityGunStatInfo.FireRate;
        private int _currentBullet;
        private float _fireTimer;
        private Inventory _inventory;
        private EntityGunStatInfo _entityGunStatInfo;
        private static int _reloadSpeedHash = Animator.StringToHash("ReloadSpeed");


        public GunItem(ItemDataSO itemData) : base(itemData)
        {
            Debug.Assert(itemData is GunDataSO, "Invalid EquipItemData");
        }

        #region attack region

        public override AttackableState CurrentAttackableState
        {
            get
            {
                if (!IsEquipped)
                    return AttackableState.NotEquipped;
                if (_currentBullet <= 0)
                    return AttackableState.NeedAmmo;
                if (!CanFire())
                    return AttackableState.Delayed;
                return AttackableState.CanAttack;
            }
        }

        public override bool UsesAnimationAttackTrigger => false;

        public override void EndAttack()
        {
            _owner.LocalEventBus.Raise(new AmmoUpdateEvent(CurrentBulletCnt, GunItemData.maxAmmoCapacity));
        }

        public override void AttackTrigger()
        {
            if (!IsEquipped || _currentBullet <= 0)
                return;

            FireShot();
            _fireTimer = 0f;
        }

        public override void UpdateAttack(AttackContext context)
        {
            if (!IsEquipped)
                return;

            IsAiming = context.IsAiming;
            float fireInterval = FireInterval;

            if (fireInterval <= 0f)
            {
                if (context.WantsAttack && _currentBullet > 0)
                {
                    FireShot();
                    _owner.LocalEventBus.Raise(new AmmoUpdateEvent(CurrentBulletCnt, GunItemData.maxAmmoCapacity));
                }
                return;
            }

            _fireTimer += Time.deltaTime;
            if (!context.WantsAttack)
            {
                _fireTimer = Mathf.Min(_fireTimer, fireInterval);
                return;
            }

            bool fired = false;

            while (_currentBullet > 0 && _fireTimer >= fireInterval)
            {
                FireShot();
                _fireTimer -= fireInterval;
                fired = true;
            }

            if (fired)
                _owner.LocalEventBus.Raise(new AmmoUpdateEvent(CurrentBulletCnt, GunItemData.maxAmmoCapacity));
        }

        private bool CanFire()
        {
            return FireInterval <= 0f || _fireTimer >= FireInterval;
        }

        private void FireShot()
        {
            if (_entityGunStatInfo.BulletReduceRate > Random.value)
                _currentBullet = Mathf.Max(_currentBullet - 1, 0);
            if (WeaponData.attackSoundID.IsValid())
                BroAudio.Play(WeaponData.attackSoundID, Dealer.gameObject.transform.position);

            WeaponObj.Attack();
        }

        #endregion

        #region reload region

        public bool CanReload
        {
            get
            {
                if (currentBulletItem == null) return CanChangeBullet();
                int cnt = _inventory.GetItemCount(currentBulletItem.ItemData);
                if (cnt <= 0)
                    return CanChangeBullet();
                if (_currentBullet == GunItemData.maxAmmoCapacity)
                    return false;
                return true;
            }
        }


        public void Reload()
        {
            List<BulletItem> bulletItems = GetValidBullets();
            if (bulletItems.Count == 0)
                return;

            currentBulletItem ??= bulletItems[0];
            int cnt = _inventory.GetItemCount(currentBulletItem.bulletDataSO);
            if (cnt <= 0)
                currentBulletItem = bulletItems[0];
            int before = _currentBullet;
            _currentBullet = Mathf.Min(_currentBullet + cnt, GunItemData.maxAmmoCapacity);
            _inventory.RemoveItem(currentBulletItem, _currentBullet - before);
        }

        private bool CanChangeBullet()
            => GetValidBullets().Count > 0;

        private List<BulletItem> GetValidBullets()
            => _inventory.GetItems<BulletItem>().Where(bullet => bullet.bulletDataSO.gunType == GunItemData.gunType)
                .ToList();

        #endregion

        #region equip region

        public override void OnEquip(Entity entity, Transform parent)
        {
            base.OnEquip(entity, parent);
            _owner = entity;
            _inventory = entity.Get<Inventory>(true);
            _entityGunStatInfo = entity.Get<EntityGunStatInfo>(true);
        }

        public override void OnUnequip(Entity entity)
        {
            base.OnUnequip(entity);
            Debug.Assert(entity == _owner, $"entity is not owner entity: {entity} owner: {_owner}");
            _owner = null;
        }

        #endregion
        
        public override void Handle(Entity entity, Transform parent)
        {
            base.Handle(entity, parent);
            entity.Get<EntityAnimator>().SetParam(_reloadSpeedHash, GunItemData.reloadTime);
            _fireTimer = 0f;
        }

        public override void UnHandle(Entity entity)
        {
            base.UnHandle(entity);
            Debug.Assert(entity == _owner, $"entity is not owner entity: {entity} owner: {_owner}");
            entity.Get<EntityAnimator>().SetParam(_reloadSpeedHash, 1);
            IsAiming = false;
        }

        public void ChangeBullet(BulletItem bulletItem)
        {
            bool isSuccess = false;
            if (currentBulletItem != null && _currentBullet != 0)
            {
                isSuccess = _inventory.TryAddItem(currentBulletItem, _currentBullet);
            }
            else
            {
                isSuccess = true;
            }

            if (isSuccess && GunItemData.gunType == bulletItem.bulletDataSO.gunType)
            {
                currentBulletItem = bulletItem;
                _currentBullet = 0;
            }
        }
    }
}
