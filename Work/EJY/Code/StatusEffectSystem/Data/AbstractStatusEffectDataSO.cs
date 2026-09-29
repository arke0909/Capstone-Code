using Scripts.Entities;
using UnityEngine;
using Code.StatusEffectSystem.StatusEffects;

namespace Code.StatusEffectSystem
{
    public abstract class AbstractStatusEffectDataSO : ScriptableObject
    {
        public bool canOverlap;
        public bool isOverWrite;

        public virtual bool CanApplyTo(Entity target, out string reason)
        {
            if (target == null)
            {
                reason = "Target is null.";
                return false;
            }

            reason = null;
            return true;
        }

        public StatusEffectInfo ApplyFlag(StatusEffectInfo info)
        {
            if (!info.UseCustomBehaviorSettings)
            {
                info.CanOverlap = canOverlap;
                info.IsOverWrite = isOverWrite;
                info.UseSharedStack = false;
                info.MaxStack = 1;
                info.StackValueMode = StatusEffectStackValueMode.None;
                info.StackDecayMode = StatusEffectStackDecayMode.ClearAllOnTimeout;
                info.StackDecayInterval = 1f;
                info.RefreshTimerOnReapply = true;
            }

            if (!info.CanOverlap)
            {
                info.UseSharedStack = false;
                info.MaxStack = 1;
            }
            else if (info.UseSharedStack)
            {
                info.MaxStack = Mathf.Max(1, info.MaxStack);
                info.StackDecayInterval = Mathf.Max(0.01f, info.StackDecayInterval);
            }

            return info;
        }
        public AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info)
        {
            AbstractStatusEffect statusEffect = CreateStatusEffectInstance(target, info);
            if (statusEffect == null)
            {
                Debug.LogError($"{name} failed to create a status effect.", this);
                return null;
            }

            statusEffect.SetStatusEffectData(this);
            return statusEffect;
        }

        protected abstract AbstractStatusEffect CreateStatusEffectInstance(Entity target, StatusEffectInfo info);
    }
}
