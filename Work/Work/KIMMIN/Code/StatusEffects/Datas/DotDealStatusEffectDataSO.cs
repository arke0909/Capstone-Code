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
        public override AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            info = ApplyFlags(info);
            return new DotDealStatusEffect(target, info);
        }
    }
}