using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    [CreateAssetMenu(fileName = "DamageStoringStatusEffectDataSO", menuName = "SO/StatusEffect/DamageStoringStatusEffect", order = 0)]
    public class DamageStoringStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            info = ApplyFlags(info);
            return new DamageStoringStatusEffect(target, info);
        }
    }
}