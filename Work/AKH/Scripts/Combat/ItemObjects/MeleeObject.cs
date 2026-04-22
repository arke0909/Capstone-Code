
using Chipmunk.ComponentContainers;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.ItemObjects
{
    public class MeleeObject : WeaponObject
    {
        [SerializeField] private OverlapDamageCaster damageCaster;
        private DamageCalcCompo _calcCompo;
        public override void InitObject(Entity owner, EquipableItem item)
        {
            base.InitObject(owner, item);
            damageCaster.InitCaster(owner);
            _calcCompo = owner.Get<DamageCalcCompo>();
            damageCaster.SetRadius((item.EquipItemData as MeleeWeaponDataSO).attackRange);
        }
        public override void Attack()
        {
            MeleeWeaponDataSO weaponData = _item.EquipItemData as MeleeWeaponDataSO;

            DamageData damageData = _calcCompo.CalculateDamage(weaponData.defaultDamage, 1, weaponData.defPierceLevel, DamageType.MELEE);
            damageCaster.CastDamage(
                damageData,
                damageCaster.transform.position,
                transform.forward,
                weaponData.attackData.knockbackMovement);
        }
    }
}
