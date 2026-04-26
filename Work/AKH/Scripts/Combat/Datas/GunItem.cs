using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using Scripts.Entities;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using Code.Combat;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;
using SHS.Scripts;
using Scripts.Combat.ItemObjects;

namespace Scripts.Combat.Datas
{
    public class GunItem : Weapon, IAttackable, IReloadable, IProjectileShooter
    {
        public GunObject GunObj => WeaponObj as GunObject;
        public BulletItem currentBulletItem;
        public GunDataSO GunItemData => EquipItemData as GunDataSO;
        public GameObject Dealer => WeaponObj.gameObject;
        public Entity Owner => _owner;
        public int CurrentBulletCnt => _currentBullet;
        public float DefaultDamage => GunItemData.defaultDamage;
        public float ProjectileSpeed => GunItemData.bulletSpeed;
        public float DamageMultiplier => BulletData.damageMultiplier;
        public int DefPierceLevel =>BulletData.defPierceLevel;
        public BulletDataSO BulletData => currentBulletItem.bulletDataSO;


        private int _currentBullet;
        private float _lastAttackTime;
        private Inventory _inventory;
        private EntityGunStatInfo _entityGunStatInfo;
        private static int _reloadSpeedHash = Animator.StringToHash("ReloadSpeed");


        public GunItem(ItemDataSO itemData) : base(itemData)
        {
            Debug.Assert(itemData is GunDataSO, "Invalid EquipItemData");
        }

        #region attack region
        AttackableState IAttackable.CurrentAttackableState { get 
            {
                if (!IsEquipped)
                    return AttackableState.NotEquipped;
                if (_currentBullet <= 0)
                    return AttackableState.NeedAmmo;
                if (Time.time - _lastAttackTime < GunItemData.fireRate * _entityGunStatInfo.FireRate)
                    return AttackableState.Delayed;
                return AttackableState.CanAttack;
            } }

        public void EnterAttack()
        {
            if (_entityGunStatInfo.BulletReduceRate > Random.value)
                _currentBullet = Mathf.Max(_currentBullet - 1, 0);
            if (WeaponData.attackSoundID.IsValid())
                BroAudio.Play(WeaponData.attackSoundID, Dealer.gameObject.transform.position);
            
            _lastAttackTime = Time.time;
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
            entity.Get<EntityAnimator>().SetParam(_reloadSpeedHash, GunItemData.reloadTime);
            _owner = entity;
            _lastAttackTime = Time.time;
            _inventory = entity.Get<Inventory>(true);
            _entityGunStatInfo = entity.Get<EntityGunStatInfo>(true);
        }

        public override void OnUnequip(Entity entity)
        {
            base.OnUnequip(entity);
            Debug.Assert(entity == _owner, $"entity is not owner entity: {entity} owner: {_owner}");
            entity.Get<EntityAnimator>().SetParam(_reloadSpeedHash, 1);
            _owner = null;
        }

        #endregion

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

        public void AttackTrigger()
        {
        }

        public void EndAnimation()
        {
        }
    }
}