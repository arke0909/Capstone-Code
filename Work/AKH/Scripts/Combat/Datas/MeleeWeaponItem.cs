using Scripts.Entities;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    public class MeleeWeaponItem : Weapon, IAttackable
    {
        public MeleeWeaponItem(ItemDataSO itemData) : base(itemData)
        {
        }

        public override AttackableState CurrentAttackableState { get
            {
                if (!IsEquipped)
                    return AttackableState.NotEquipped;
                return AttackableState.CanAttack;
            } }

        public override void AttackTrigger()
        {
            base.AttackTrigger();
            WeaponObj.Attack();
        }
    }
}
