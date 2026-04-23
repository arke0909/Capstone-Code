using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Work.EJY.Code.StatusEffectSystem.Data
{
    [CreateAssetMenu(fileName = "Shield Data", menuName = "SO/StatusEffect/ShieldData", order = 0)]
    public class ShieldStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            info = ApplyFlags(info);
            return new ShieldStatusEffect(target, info);
        }
    }
}