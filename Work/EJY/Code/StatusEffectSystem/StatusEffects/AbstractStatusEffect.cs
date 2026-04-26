using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public abstract class AbstractStatusEffect
    {
        public BuffSO KeySO { get; protected set; }
        public StatusEffectEnum StatusEffectEnum { get; protected set; }
        public int Priority { get; protected set; }
        public float CurrentTime { get; protected set; }
        public float RemainingTime => Mathf.Max(_applyTime - CurrentTime, 0f);

        protected Entity _target;
        protected float _value;
        protected bool _isApplying;
        protected float _applyTime;

        public AbstractStatusEffect(Entity target, StatusEffectInfo statusEffectInfo)
        {
            _target = target;
            KeySO = statusEffectInfo.KeySO;
            StatusEffectEnum = statusEffectInfo.StatusEffect;
            Priority = statusEffectInfo.Priority;
            _applyTime = statusEffectInfo.ApplyTime;
            _value = statusEffectInfo.Value;
            CurrentTime = 0;
        }
        
        public void SetValue(float value) => _value = value;

        protected virtual void ResetStatusEffect()
        {
        }

        public virtual bool UpdateStatusEffect(Entity entity)
        {
            CurrentTime += Time.deltaTime;

            if (!_isApplying || CurrentTime >= _applyTime)
                return false;
            return true;
        }

        public virtual void ApplyStatusEffect(Entity entity)
        {
            CurrentTime = 0;
            _isApplying = true;
        }
        
        public abstract void ReleaseStatusEffect(Entity entity);

        public void SetRemainingTime(float applyTime)
        {
            _applyTime = Mathf.Max(0f, applyTime);
            CurrentTime = 0;
            ResetStatusEffect();
        }

        public void SetStrongerValue(StatusEffectInfo info)
        {
            Priority = info.Priority;
            _value = info.Value;
            SetRemainingTime(info.ApplyTime);
            ReleaseStatusEffect(_target);
            ApplyStatusEffect(_target);
        }
    }
}