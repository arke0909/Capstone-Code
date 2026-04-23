using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    [CreateAssetMenu(fileName = "MeleeWeaponDataSO", menuName = "SO/Item/MeleeWeaponData", order = 0)]
    public class MeleeWeaponDataSO : WeaponDataSO
    {
        public AttackDataSO attackData;
        public int defPierceLevel = 0;
        public override ItemCreateData CreateItem()
            => new ItemCreateData(new MeleeWeaponItem(this), maxStack);
    }
}
