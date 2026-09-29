using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem
{
    [CreateAssetMenu(fileName = "HealthRegenStatusEffectData", menuName = "SO/StatusEffect/HealthRegenStatusEffectData", order = 0)]
    public class HealthRegenStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        protected override AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info)
        {
            return new HealthRegenStatusEffect(target, info);
        }

        public override bool CanApplyTo(Entity target, out string reason)
        {
            if (!base.CanApplyTo(target, out reason))
                return false;

            if (target.Get<HealthCompo>() == null)
            {
                reason = $"{target.name} has no HealthCompo.";
                return false;
            }

            reason = null;
            return true;
        }
    }
}
