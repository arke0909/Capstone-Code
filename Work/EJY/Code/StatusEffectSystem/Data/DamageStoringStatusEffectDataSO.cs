using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    [CreateAssetMenu(fileName = "DamageStoringStatusEffectDataSO", menuName = "SO/StatusEffect/DamageStoringStatusEffect", order = 0)]
    public class DamageStoringStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        protected override AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info)
        {
            return new DamageStoringStatusEffect(target, info);
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
