using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Scripts.Entities;
using UnityEngine;
using Code.StatusEffectSystem.StatusEffects;

namespace Code.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "StatStatusEffectData", menuName = "SO/StatusEffect/StatStatusEffectData", order = 0)]

    public class StatStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        public StatSO targetStat;
        public bool isMultiplicationOperation;
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            info.CanOverlap = canOverlap;
            StatStatusEffect statusEffect = new StatStatusEffect(target,info,targetStat);

            float value = info.Value;
            
            if (info.IsPercent)
            {
                var stat = target.Get<StatOverrideBehavior>().GetStat(targetStat);
                float statValue = isMultiplicationOperation ? stat.Value : stat.BaseValue;
                
                value *= statValue;
            }
            
            statusEffect.SetValue(value);
            return statusEffect;
        }
    }
}