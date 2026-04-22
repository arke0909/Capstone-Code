using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "HealthRegenStatusEffectData", menuName = "SO/StatusEffect/HealthRegenStatusEffectData", order = 0)]
    public class HealthRegenStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            info.CanOverlap = canOverlap;
            return new HealthRegenStatusEffect(target, info);
        }
    }
}