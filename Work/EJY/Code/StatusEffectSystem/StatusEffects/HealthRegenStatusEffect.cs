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
            _restoreAmountPerTick = _value / _remainingTicks;
        }

        protected override void ResetStatusEffect()
        {
            RecalculateTicks();
        }

        public override bool UpdateStatusEffect(Entity entity)
        {
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= _tick && _remainingTicks > 0)
            {
                _tickTimer -= _tick;
                _remainingTicks--;
                _targetHealth.CurrentValue += _restoreAmountPerTick;
            }

            return base.UpdateStatusEffect(entity);
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
        }
    }
}
