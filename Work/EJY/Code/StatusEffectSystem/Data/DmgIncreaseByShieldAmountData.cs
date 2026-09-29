using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.Data
{
    [CreateAssetMenu(fileName = "DmgIncreaseByShieldData", menuName = "SO/StatusEffect/DmgIncreaseByShieldData", order = 0)]
    public class DmgIncreaseByShieldAmountData : StatStatusEffectDataSO
    {
        protected override AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info)
        {
            return new DmgIncrByShieldAmountStatusEffect(target, info, targetStat);
        }
    }
}
