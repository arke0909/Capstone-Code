using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public abstract class AbstractStatusEffect
    {
        public BuffSO KeySO { get; protected set; }
        public StatusEffectEnum StatusEffectEnum { get; protected set; }
        public int Level { get; protected set; }
        public float CurrentTime { get; protected set; }

        protected Entity _target;
        protected float _value;
        protected bool _isApplying;
        protected float _applyTime;

        public AbstractStatusEffect(Entity target, StatusEffectInfo statusEffectInfo)
        {
            _target = target;
            KeySO = statusEffectInfo.KeySO;
            StatusEffectEnum = statusEffectInfo.StatusEffect;
            Level = statusEffectInfo.Level;
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
    
            if(!_isApplying || CurrentTime >= _applyTime)
                return false; // 더 이상 유지되지 않음 (제거 대상)
            return true; // 계속 유지됨
        }

        public virtual void ApplyStatusEffect(Entity entity)
        {
            CurrentTime = 0;
            _isApplying = true;  
        }
        
        public abstract void ReleaseStatusEffect(Entity entity);

        public void SetRemainingTime(float applyTime)
        {
            _applyTime = applyTime;
            CurrentTime = 0;
            ResetStatusEffect();
        }
    }
}