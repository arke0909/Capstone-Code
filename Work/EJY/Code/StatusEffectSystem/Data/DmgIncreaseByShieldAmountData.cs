using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Work.EJY.Code.StatusEffectSystem.Data
{
    [CreateAssetMenu(fileName = "DmgIncreaseByShieldData", menuName = "SO/StatusEffect/DmgIncreaseByShieldData", order = 0)]
    public class DmgIncreaseByShieldAmountData : StatStatusEffectDataSO
    {
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            return new DmgIncrByShieldAmountStatusEffect(target, info, targetStat);
        }
    }
}