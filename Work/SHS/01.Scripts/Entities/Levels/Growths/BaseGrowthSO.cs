using AYellowpaper.SerializedCollections;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Entities.Levels.Growths
{
    public abstract class BaseGrowthSO : ScriptableObject
    {
        public void ApplyGrowth(Entity entity)
        {
            ApplyGrowthEffect(entity);
        }

        protected abstract void ApplyGrowthEffect(Entity entity);
    }
}