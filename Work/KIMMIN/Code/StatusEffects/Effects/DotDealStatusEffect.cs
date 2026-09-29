using Chipmunk.ComponentContainers;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Code.StatusEffectSystem;
using Code.StatusEffectSystem.StatusEffects;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Work.Code.StatusEffects.Effects
{
    public class DotDealStatusEffect : AbstractStatusEffect
    {
        private float _tick = 0.5f;
        private float _tickTimer = 0f;
        private float _tickInterval;
        private float _damagePerTick = 0f;
        private int _remainingTicks;
        private HealthCompo _targetHealth;

        public DotDealStatusEffect(Entity target, StatusEffectInfo statusEffectInfo) : base(target, statusEffectInfo)
        {
            _targetHealth = target.Get<HealthCompo>();
            Debug.Assert(_targetHealth != null, "Target has no health compo");
            CalcTick();
        }

        private void CalcTick()
        {
            _tickTimer = 0f;
            _remainingTicks = Mathf.Max(1, Mathf.CeilToInt(_applyTime / _tick));
            _tickInterval = Mathf.Max(_applyTime / _remainingTicks, Mathf.Epsilon);
            _damagePerTick = _value / _remainingTicks;
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
                DamageData damageData = new()
                {
                    damage = _damagePerTick,
                    damageType = DamageType.DOT,
                    defPierceLevel = 1
                };
                _targetHealth.ApplyDamage(damageData);
            }

            return base.UpdateStatusEffect(entity);
        }

        public override void ExtendDuration(float additionalTime)
        {
            if (additionalTime <= 0f)
                return;

            float remainingDamage = _damagePerTick * _remainingTicks;
            float tickProgress = _tickInterval > Mathf.Epsilon
                ? Mathf.Clamp01(_tickTimer / _tickInterval)
                : 0f;

            base.ExtendDuration(additionalTime);

            _remainingTicks = Mathf.Max(1, Mathf.CeilToInt(RemainingTime / _tick));
            _tickInterval = Mathf.Max(RemainingTime / _remainingTicks, Mathf.Epsilon);
            _damagePerTick = remainingDamage / _remainingTicks;
            _tickTimer = tickProgress * _tickInterval;
        }

        protected override void ResetStatusEffect()
        {
            CalcTick();
        }

        protected override void OnValueChanged()
        {
            CalcTick();
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
            _isApplying = false;
        }
    }
}
