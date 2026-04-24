using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Scripts.Combat;
using Scripts.Entities;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class DmgIncrByShieldAmountStatusEffect : StatStatusEffect
    {
        private ShieldCompo _shieldCompo;
        private float _shieldScale;
        
        public DmgIncrByShieldAmountStatusEffect(Entity target, StatusEffectInfo statusEffectInfo, StatSO statSO) : base(target, statusEffectInfo, statSO)
        {
            _shieldCompo = target.Get<ShieldCompo>();
            _shieldScale = statusEffectInfo.Value;
        }

        public override void ApplyStatusEffect(Entity entity)
        {
            if (_targetStatSO == null || _targetStat == null || _shieldCompo == null)
            {
                _isApplying = false;
                return;
            }

            CurrentTime = 0f;
            _isApplying = true;
            _shieldCompo.OnShieldAmountChanged += HandleShieldAmountChange;
            HandleShieldAmountChange(_shieldCompo.CurrentShieldAmount);
        }

        private void HandleShieldAmountChange(float shieldAmount)
        {
            var targetStat = _targetStat.GetStat(_targetStatSO);
            targetStat.RemoveModifier(this);
            float addValue = _shieldScale * shieldAmount;
            targetStat.AddValueModifier(this, addValue);
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            if (_shieldCompo != null)
                _shieldCompo.OnShieldAmountChanged -= HandleShieldAmountChange;

            base.ReleaseStatusEffect(entity);
        }
    }
}
