using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "Additional Damage Status Effect", menuName = "SO/StatusEffect/AdditionalDamage", order = 0)]
    public class AdditionalDamageStatusEffectData : AbstractStatusEffectDataSO
    {
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            info.CanOverlap = canOverlap;
            return new AdditionalDamageStatusEffect(target, info);
        }
    }
}