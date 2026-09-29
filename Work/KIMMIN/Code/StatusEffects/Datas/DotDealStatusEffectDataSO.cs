using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Entities;
using UnityEngine;
using Work.Code.StatusEffects.Effects;

namespace Work.Code.StatusEffects.Datas
{
    [CreateAssetMenu(fileName = "DotDealStatusEffectData", menuName = "SO/StatusEffect/DotDealStatusEffectData", order = 0)]
    public class DotDealStatusEffectDataSO : AbstractStatusEffectDataSO
    {
        protected override AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info)
        {
            return new DotDealStatusEffect(target, info);
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
