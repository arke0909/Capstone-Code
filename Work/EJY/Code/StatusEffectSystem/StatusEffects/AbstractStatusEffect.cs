using Scripts.Entities;
using UnityEngine;

namespace Code.StatusEffectSystem.StatusEffects
{
    public abstract class AbstractStatusEffect
    {
        public int CreateDataIndex { get; protected set; }
        public BuffSO KeySO { get; protected set; }
        public StatusEffectEnum StatusEffectEnum { get; protected set; }
        public int Priority { get; protected set; }
        public float CurrentTime { get; protected set; }
        public float RemainingTime =>
            _useSharedStack && _stackDecayMode == StatusEffectStackDecayMode.DecreaseOneByOne
                ? Mathf.Max(_stackDecayInterval - _stackTimer, 0f)
                : Mathf.Max(_applyTime - CurrentTime, 0f);
        public int StackCount { get; protected set; }
        public int MaxStack { get; protected set; }

        protected Entity _target;
        protected float _value;
        protected float _baseValue;
        protected bool _isApplying;
        protected float _applyTime;
        protected bool _useSharedStack;
        protected StatusEffectStackValueMode _stackValueMode;
        protected StatusEffectStackDecayMode _stackDecayMode;
        protected float _stackDecayInterval;
        protected bool _refreshTimerOnReapply;
        protected float _stackTimer;

        public AbstractStatusEffect(Entity target, StatusEffectInfo statusEffectInfo)
        {
            _target = target;
            CreateDataIndex = statusEffectInfo.CreateDataIndex;
            KeySO = statusEffectInfo.KeySO;
            StatusEffectEnum = statusEffectInfo.StatusEffect;
            Priority = statusEffectInfo.Priority;
            _applyTime = statusEffectInfo.ApplyTime;
            _value = statusEffectInfo.Value;
            _baseValue = statusEffectInfo.Value;
            CurrentTime = 0;
            StackCount = 1;
            MaxStack = Mathf.Max(1, statusEffectInfo.MaxStack);
            _useSharedStack = statusEffectInfo.UseSharedStack;
            _stackValueMode = statusEffectInfo.StackValueMode;
            _stackDecayMode = statusEffectInfo.StackDecayMode;
            _stackDecayInterval = Mathf.Max(0.01f, statusEffectInfo.StackDecayInterval);
            _refreshTimerOnReapply = statusEffectInfo.RefreshTimerOnReapply;
            _stackTimer = 0f;
        }
        
        public void SetValue(float value)
        {
            _baseValue = value;
            RecalculateValue();
        }

        public void AddSharedStack(StatusEffectInfo info)
        {
            if (!_useSharedStack)
                return;

            MaxStack = Mathf.Max(1, info.MaxStack);
            StackCount = Mathf.Clamp(StackCount + 1, 1, MaxStack);

            if (_refreshTimerOnReapply)
            {
                if (_stackDecayMode == StatusEffectStackDecayMode.DecreaseOneByOne)
                    _stackTimer = 0f;
                else
                    SetRemainingTime(info.ApplyTime);
            }

            RecalculateValue();
        }

        protected virtual void ResetStatusEffect()
        {
        }

        protected virtual float CalculateStackedValue()
        {
            if (!_useSharedStack)
                return _baseValue;

            switch (_stackValueMode)
            {
                case StatusEffectStackValueMode.Linear:
                    return _baseValue * StackCount;

                default:
                    return _baseValue;
            }
        }

        protected virtual void OnValueChanged()
        {
        }

        protected void RecalculateValue()
        {
            _value = CalculateStackedValue();
            OnValueChanged();
        }

        public virtual bool UpdateStatusEffect(Entity entity)
        {
            if (!_isApplying)
                return false;

            if (_useSharedStack && _stackDecayMode == StatusEffectStackDecayMode.DecreaseOneByOne)
            {
                _stackTimer += Time.deltaTime;

                while (_stackTimer >= _stackDecayInterval && StackCount > 0)
                {
                    _stackTimer -= _stackDecayInterval;
                    StackCount--;
                    RecalculateValue();

                    if (StackCount <= 0)
                        return false;
                }

                return true;
            }

            CurrentTime += Time.deltaTime;

            if (CurrentTime >= _applyTime)
                return false;
            return true;
        }

        public virtual void ApplyStatusEffect(Entity entity)
        {
            CurrentTime = 0;
            _stackTimer = 0f;
            _isApplying = true;
        }
        
        public abstract void ReleaseStatusEffect(Entity entity);

        public void SetRemainingTime(float applyTime)
        {
            _applyTime = Mathf.Max(0f, applyTime);
            CurrentTime = 0;
            _stackTimer = 0f;
            ResetStatusEffect();
        }

        public void SetStrongerValue(StatusEffectInfo info)
        {
            Priority = info.Priority;
            _baseValue = info.Value;
            RecalculateValue();
            SetRemainingTime(info.ApplyTime);
            ReleaseStatusEffect(_target);
            ApplyStatusEffect(_target);
        }
    }
}
