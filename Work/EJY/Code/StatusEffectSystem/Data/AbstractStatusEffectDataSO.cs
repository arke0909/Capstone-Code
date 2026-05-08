using Scripts.Entities;
using UnityEngine;
using Code.StatusEffectSystem.StatusEffects;

namespace Code.StatusEffectSystem
{
    public abstract class AbstractStatusEffectDataSO : ScriptableObject
    {
        public int idx;
        public string StatusEffectName;
        public bool canOverlap;
        public bool isOverWrite;

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
        public abstract AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info);
    }
}
