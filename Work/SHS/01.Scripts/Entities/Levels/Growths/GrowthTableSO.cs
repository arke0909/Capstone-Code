using System;
using System.Collections.Generic;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Entities.Levels.Growths
{
    [CreateAssetMenu(fileName = "GrowthTableSO", menuName = "ScriptableObject/GrowthTableSO", order = 0)]
    public class GrowthTableSO : ScriptableObject
    {
        [Serializable]
        private struct GrowthByLevel
        {
            public BaseGrowthSO[] growthByLevel;
        }

        [SerializeField] private List<GrowthByLevel> growthTable = new();

        public void ApplyGrowths(Entity entity, int level)
        {
            if (level > growthTable.Count) return;
            GrowthByLevel growthByLevel = growthTable[level - 1];
            foreach (var growth in growthByLevel.growthByLevel)
            {
                growth.ApplyGrowth(entity);
            }
        }
    }
}