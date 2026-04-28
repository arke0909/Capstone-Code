using Chipmunk.ComponentContainers;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.Items
{
    public abstract class Weapon : HandItem, IAttackable
    {
        private static int _attackSpeedHash = Animator.StringToHash("AttackSpeed");
        
        public WeaponDataSO WeaponData { get; private set; }
        public WeaponObject WeaponObj => ItemObject as WeaponObject;
        
        public Weapon(ItemDataSO itemData) : base(itemData)
        {
            Debug.Assert(itemData is WeaponDataSO, "Invalid EquipItemData");
            WeaponData = itemData as WeaponDataSO;
        }
        public override void OnEquip(Entity entity,Transform parent)
        {
            base.OnEquip(entity, parent);
            EntityAnimator animator = entity.Get<EntityAnimator>();
            animator.ChangeAnimatorController(WeaponData.controller);
            animator.SetParam(_attackSpeedHash, WeaponData.attackSpeed);
        }
        public override void OnUnequip(Entity entity)
        {
            base.OnUnequip(entity);
            EntityAnimator animator = entity.Get<EntityAnimator>();
            animator.SetDefaultController();
            animator.SetParam(_attackSpeedHash, 1);
        }

        public GameObject Dealer => WeaponObj.gameObject;
        public Entity Owner => _owner;
        public abstract AttackableState CurrentAttackableState { get; }
        
        public virtual void EnterAttack()
        {
        }

        public virtual void AttackTrigger()
        {
        }

        public virtual void EndAnimation()
        {
        }
    }
}