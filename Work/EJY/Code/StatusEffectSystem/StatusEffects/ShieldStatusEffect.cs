using Chipmunk.ComponentContainers;
using Scripts.Combat;
using Scripts.Entities;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class ShieldStatusEffect : AbstractStatusEffect
    {

        public ShieldStatusEffect(Entity target, StatusEffectInfo statusEffectInfo) : base(target, statusEffectInfo)
        {
            _shieldCompo = target.Get<ShieldCompo>();
            _shieldAmount = statusEffectInfo.Value;
        }

        private ShieldCompo _shieldCompo;
        private ShieldInstance _shieldInstance;
        private float _shieldAmount;

        public override void ApplyStatusEffect(Entity entity)
        {
            base.ApplyStatusEffect(entity);
            _shieldInstance = _shieldCompo.AddShield(_shieldAmount, () => _isApplying = false);
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            _isApplying = false;
            _shieldCompo.RemoveShield(_shieldInstance);
        }

        public override void SetStrongerValue(StatusEffectInfo info)
        {
            Priority = info.Priority;
            SetValue(info.Value);
            SetRemainingTime(info.ApplyTime);
        }

        protected override void OnValueChanged()
        {
            _shieldAmount = _value;

            if (!_isApplying)
                return;

            _shieldCompo.RemoveShield(_shieldInstance);
            _isApplying = true;
            _shieldInstance = _shieldCompo.AddShield(_shieldAmount, () => _isApplying = false);
        }
    }
}
