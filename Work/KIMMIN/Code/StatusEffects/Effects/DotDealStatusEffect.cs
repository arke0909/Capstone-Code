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
            _damagePerTick = _value / _remainingTicks;
        }

        public override bool UpdateStatusEffect(Entity entity)
        {
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= _tick && _remainingTicks > 0)
            {
                _tickTimer -= _tick;
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

        protected override void ResetStatusEffect()
        {
            CalcTick();
        }

        public override void ReleaseStatusEffect(Entity entity)
        {
        }
    }
}
