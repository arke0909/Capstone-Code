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
        
        public abstract AbstractStatusEffect CreateStatusEffect(Entity target, StatusEffectInfo info);
    }
}