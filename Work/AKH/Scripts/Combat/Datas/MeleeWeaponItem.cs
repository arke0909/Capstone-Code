using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    public class MeleeWeaponItem : Weapon, IAttackable
    {
        private int _comboCounter;

        public GameObject Dealer => WeaponObj.gameObject;
        public Entity Owner => null;
        
        public MeleeWeaponItem(ItemDataSO itemData) : base(itemData)
        {
        }

        public AttackableState CurrentAttackableState { get
            {
                if (!IsEquipped)
                    return AttackableState.NotEquipped;
                return AttackableState.CanAttack;
            } }

        public void EnterAttack()
        {
        }

        public void AttackTrigger()
        {
            WeaponObj.Attack();
        }

        public void EndAnimation()
        {
        }
    }
}
