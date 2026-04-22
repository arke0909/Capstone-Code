using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    public class ThrowableItem : Weapon, IAttackable
    {
        private Inventory _inventory;
        
        public GameObject Dealer => WeaponObj.gameObject;
        public Entity Owner => _entity;

        private Entity _entity;
        
        
        public ThrowableItem(ItemDataSO itemData) : base(itemData)
        {
        }

        public AttackableState CurrentAttackableState { get
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
            _entity = entity;
        }

        public override void OnUnequip(Entity entity)
        {
            base.OnUnequip(entity);
            _inventory = null;
        }

        public void EnterAttack()
        {
        }

        public void AttackTrigger()
        {
            if(!_inventory.RemoveItem(this, 1, false)) return;
            WeaponObj?.Attack();
        }

        public void EndAnimation()
        {
        }
    }
}
