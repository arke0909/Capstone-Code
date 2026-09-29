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

        protected override AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info)
        {
            return new StatStatusEffect(target, info, targetStat, true);
        }

        public override bool CanApplyTo(Entity target, out string reason)
        {
            if (!base.CanApplyTo(target, out reason))
                return false;

            if (targetStat == null)
            {
                reason = $"{name} has no target stat.";
                return false;
            }

            StatOverrideBehavior statBehavior = target.Get<StatOverrideBehavior>();
            if (statBehavior == null)
            {
                reason = $"{target.name} has no StatOverrideBehavior.";
                return false;
            }

            if (!statBehavior.TryGetStat(targetStat, out _))
            {
                reason = $"{target.name} does not have {targetStat.name} registered.";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
