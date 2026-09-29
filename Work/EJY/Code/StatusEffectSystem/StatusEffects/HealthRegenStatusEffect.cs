using Chipmunk.ComponentContainers;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public class HealthRegenStatusEffect : AbstractStatusEffect
    {
        private float _tick = 0.5f;
        private float _tickTimer;
        private float _tickInterval;
        private float _restoreAmountPerTick;
        private int _remainingTicks;
        private HealthCompo _targetHealth;

        public HealthRegenStatusEffect(Entity target, StatusEffectInfo statusEffectInfo) : base(target,
            statusEffectInfo)
        {
            _targetHealth = target.Get<HealthCompo>();
            Debug.Assert(_targetHealth != null, "Target has no health compo");
            RecalculateTicks();
        }

        private void RecalculateTicks()
        {
            _tickTimer = 0f;
            _remainingTicks = Mathf.Max(1, Mathf.CeilToInt(_applyTime / _tick));
            _tickInterval = Mathf.Max(_applyTime / _remainingTicks, Mathf.Epsilon);
            _restoreAmountPerTick = _value / _remainingTicks;
        }

        protected override void ResetStatusEffect()
        {
            RecalculateTicks();
        }

        protected override void OnValueChanged()
        {
            RecalculateTicks();
        }

        public override bool UpdateStatusEffect(Entity entity)
        {
            if (!_isApplying)
                return false;

            _tickTimer += Time.deltaTime;

            while (_isApplying && _tickTimer >= _tickInterval && _remainingTicks > 0)
            {
                _tickTimer -= _tickInterval;
                _remainingTicks--;
                _targetHealth.Heal(_restoreAmountPerTick, false);
            }

            return base.UpdateStatusEffect(entity);
        }

        public override void ExtendDuration(float additionalTime)
        {
            if (additionalTime <= 0f)
                return;

            float remainingRestoreAmount = _restoreAmountPerTick * _remainingTicks;
            float tickProgress = _tickInterval > Mathf.Epsilon
                ? Mathf.Clamp01(_tickTimer / _tickInterval)
                : 0f;

            base.ExtendDuration(additionalTime);

            _remainingTicks = Mathf.Max(1, Mathf.CeilToInt(RemainingTime / _tick));
            _tickInterval = Mathf.Max(RemainingTime / _remainingTicks, Mathf.Epsilon);
            _restoreAmountPerTick = remainingRestoreAmount / _remainingTicks;
            _tickTimer = tickProgress * _tickInterval;
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            _isApplying = false;
        }
    }
}
