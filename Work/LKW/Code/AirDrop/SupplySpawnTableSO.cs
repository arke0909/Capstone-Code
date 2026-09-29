using System;
using System.Collections.Generic;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.AirDrop
{
    [Serializable]
    public class SupplyDropDefinition
    {
        public string supplyName;
        public PoolItemSO airdropPool;
        public Sprite minimapIcon;
        public SupplyRewardTableSO rewardTable;

        public string DisplayName => string.IsNullOrWhiteSpace(supplyName) ? "Supply" : supplyName;
    }

    [Serializable]
    public class SupplySpawnDayRule
    {
        [Min(1)] public int startDay = 1;
        [Tooltip("0 means this rule has no last-day limit.")]
        [Min(0)] public int endDay;
        public List<SupplyDropDefinition> drops = new();

        public bool ContainsDay(int day)
        {
            return day >= startDay && (endDay <= 0 || day <= endDay);
        }
    }

    [CreateAssetMenu(fileName = "SupplySpawnTable", menuName = "SO/AirDrop/SupplySpawnTable")]
    public class SupplySpawnTableSO : ScriptableObject
    {
        [SerializeField] private List<SupplySpawnDayRule> dayRules = new();

        public IReadOnlyList<SupplySpawnDayRule> DayRules => dayRules;

        public IReadOnlyList<SupplyDropDefinition> GetDrops(int day)
        {
            if (dayRules == null || dayRules.Count == 0)
                return Array.Empty<SupplyDropDefinition>();

            SupplySpawnDayRule fallback = null;

            foreach (SupplySpawnDayRule rule in dayRules)
            {
                if (rule == null)
                    continue;

                fallback ??= rule;

                if (rule.ContainsDay(day))
                    return rule.drops != null ? rule.drops : Array.Empty<SupplyDropDefinition>();
            }

            return fallback?.drops != null ? fallback.drops : Array.Empty<SupplyDropDefinition>();
        }
    }
}
