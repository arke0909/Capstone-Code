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
            info.CanOverlap = canOverlap;
            info.IsOverWrite = isOverWrite;
            return info;
        }
        public abstract AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info);
    }
}