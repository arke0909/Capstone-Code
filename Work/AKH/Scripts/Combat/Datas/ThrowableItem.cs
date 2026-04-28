using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using Scripts.Entities;
using SHS.Scripts;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    public class ThrowableItem : Weapon, IAttackable,IProjectileShooter
    {
        private Inventory _inventory;
        
        public ThrowableDataSO ThrowableData { get; private set; }

        public float DefaultDamage => ThrowableData.defaultDamage;

        public float ProjectileSpeed => ThrowableData.speedCurve.Evaluate(0);

        public float DamageMultiplier =>ThrowableData.damageMultiplier;

        public int DefPierceLevel => ThrowableData.defPierceLevel;

        public ThrowableItem(ItemDataSO itemData) : base(itemData)
        {
            Debug.Assert(itemData is ThrowableDataSO a, $"Invalid ThrowableItem : {itemData}");
            ThrowableData = itemData as ThrowableDataSO; 
        }

        public override AttackableState CurrentAttackableState { get
            {
                if (!IsEquipped)
                    return AttackableState.NotEquipped;
                if(_inventory?.GetItemCount(ItemData) <= 0)
                    return AttackableState.NeedStack;
                return AttackableState.CanAttack;
            } }

        public override void OnEquip(Entity entity, Transform parent)
        {
            base.OnEquip(entity, parent);
            _inventory = entity.Get<Inventory>(true);
        }

        public override void OnUnequip(Entity entity)
        {
            base.OnUnequip(entity);
            _inventory = null;
        }

        public override void AttackTrigger()
        {
            base.AttackTrigger();
            if(!_inventory.RemoveItem(this, 1, false)) return;
            WeaponObj?.Attack();
        }
    }
}
