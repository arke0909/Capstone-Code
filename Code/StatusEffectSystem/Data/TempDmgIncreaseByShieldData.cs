using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Work.EJY.Code.StatusEffectSystem.Data
{
    // 임시
    [CreateAssetMenu(fileName = "DmgIncreaseByShieldData", menuName = "SO/StatusEffect/DmgIncreaseByShieldData", order = 0)]
    public class TempDmgIncreaseByShieldData : StatStatusEffectDataSO
    {
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            return new TempDmgIncrByShieldStatusEffect(target, info, targetStat);
        }
    }
}