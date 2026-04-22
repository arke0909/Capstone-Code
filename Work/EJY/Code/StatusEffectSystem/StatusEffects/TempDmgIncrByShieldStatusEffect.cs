using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Scripts.Combat;
using Scripts.Entities;

namespace Code.StatusEffectSystem.StatusEffects
{
    // 임시

    public class TempDmgIncrByShieldStatusEffect : StatStatusEffect
    {
        private ShieldCompo _shieldCompo;
        private float _value;
        
        public TempDmgIncrByShieldStatusEffect(Entity target, StatusEffectInfo statusEffectInfo, StatSO statSO) : base(target, statusEffectInfo, statSO)
        {
            _shieldCompo = target.Get<ShieldCompo>();
            _value = statusEffectInfo.Value;
        }

        public override void ApplyStatusEffect(Entity entity)
        {
            base.ApplyStatusEffect(entity);
            _shieldCompo.OnShieldAmountChanged += HandleShieldAmountChange;
        }

        private void HandleShieldAmountChange(float shieldAmount)
        {
            var targetStat = _targetStat.GetStat(_targetStatSO);
            targetStat.RemoveModifier(this);
            float addValue = _value * shieldAmount;
            targetStat.AddValueModifier(this, addValue);   
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            _shieldCompo.OnShieldAmountChanged -= HandleShieldAmountChange;
        }
    }
}