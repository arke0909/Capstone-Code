using Chipmunk.ComponentContainers;
using Code.Items.ItemInfo;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using Scripts.Entities;
using UnityEngine;

namespace Code.Items
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

        public override void Handle(Entity entity, Transform parent)
        {
            base.Handle(entity, parent);
            EntityAnimator animator = entity.Get<EntityAnimator>();
            animator.ChangeAnimatorController(WeaponData.controller);
            animator.SetParam(_attackSpeedHash, WeaponData.attackSpeed);
        }

        public override void UnHandle(Entity entity)
        {
            base.UnHandle(entity);
            EntityAnimator animator = entity.Get<EntityAnimator>();
            animator.SetDefaultController();
            animator.SetParam(_attackSpeedHash, 1);
        }

        public GameObject Dealer => WeaponObj.gameObject;
        public Entity Owner => _owner;
        public abstract AttackableState CurrentAttackableState { get; }
        public virtual bool UsesAnimationAttackTrigger => true;
        
        public virtual void EnterAttack()
        {
        }

        public virtual void AttackTrigger()
        {
        }

        public virtual void UpdateAttack(AttackContext context)
        {
        }

        public virtual void EndAttack()
        {
        }
    }
}
