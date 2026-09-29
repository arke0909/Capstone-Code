using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.Data
{
    [CreateAssetMenu(fileName = "Shield Data", menuName = "SO/StatusEffect/ShieldData", order = 0)]
    public class ShieldStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        protected override AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info)
        {
            return new ShieldStatusEffect(target, info);
        }
    }
}
