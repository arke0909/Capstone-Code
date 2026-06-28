using System;
using System.Collections.Generic;
using Code.Items.ItemInfo;
using UnityEngine;

namespace Code.AirDrop
{
    [Serializable]
    public class SupplyRarityWeight
    {
        public Rarity rarity = Rarity.Common;
        [Min(0f)] public float weight = 1f;
    }

    [Serializable]
    public class SupplyItemTypeWeight
    {
        public ItemType itemType = ItemType.None;
        [Min(0f)] public float weight = 1f;
    }

    [Serializable]
    public class SupplyDayRule
    {
        [Min(1)] public int startDay = 1;
        [Tooltip("0이면 마지막 일차 제한이 없습니다.")]
        [Min(0)] public int endDay;

        [Header("Reward Count")]
        [Min(0)] public int minRewardCount;
        [Min(0)] public int maxRewardCount;

        [Header("Budget")]
        [Tooltip("0 이하면 예산 제한 없이 아이템 수만 기준으로 뽑습니다.")]
        [Min(0)] public int supplyBudget;
        [Min(0.01f)] public float stackMultiplier = 1f;

        [Header("Weights")]
        public List<SupplyRarityWeight> rarityWeights = new();
        public List<SupplyItemTypeWeight> itemTypeWeights = new();

        public bool ContainsDay(int day)
        {
            return day >= startDay && (endDay <= 0 || day <= endDay);
        }

        public int GetRewardCount()
        {
            int min = Mathf.Max(0, minRewardCount);
            int max = Mathf.Max(min, maxRewardCount);
            return UnityEngine.Random.Range(min, max + 1);
        }
    }

    [Serializable]
    public class SupplyRewardEntry
    {
        public ItemDataSO itemData;

        [Min(1)] public int minDay = 1;
        [Tooltip("0이면 마지막 일차 제한이 없습니다.")]
        [Min(0)] public int maxDay;

        [Tooltip("체크하면 보급마다 이 조건의 후보 중 최소 하나를 먼저 뽑습니다.")]
        public bool guaranteed;
        [Min(0f)] public float weight = 1f;
        [Min(1)] public int cost = 1;
        [Tooltip("0이면 한 보급에서 중복 횟수 제한이 없습니다.")]
        [Min(0)] public int maxPickCount = 1;

        [Min(1)] public int minStack = 1;
        [Min(1)] public int maxStack = 1;

        public Rarity Rarity => itemData != null ? itemData.rarity : Rarity.None;
        public ItemType ItemType => itemData != null ? itemData.itemType : ItemType.None;

        public bool IsAvailable(int day)
        {
            return itemData != null
                   && weight > 0f
                   && day >= minDay
                   && (maxDay <= 0 || day <= maxDay);
        }

        public int GetStack(float multiplier)
        {
            if (itemData == null)
                return 0;

            int min = Mathf.Max(1, minStack);
            int max = Mathf.Max(min, maxStack);
            int stack = UnityEngine.Random.Range(min, max + 1);
            stack = Mathf.CeilToInt(stack * Mathf.Max(0.01f, multiplier));
            return Mathf.Clamp(stack, 1, Mathf.Max(1, itemData.maxStack));
        }
    }

    [Serializable]
    public class SupplyGuaranteedGroup
    {
        public string groupName;

        [Min(1)] public int minDay = 1;
        [Tooltip("0 means this group has no last-day limit.")]
        [Min(0)] public int maxDay;

        [Min(1)] public int pickCount = 1;
        public List<SupplyRewardEntry> rewardEntries = new();

        public bool IsAvailable(int day)
        {
            return day >= minDay && (maxDay <= 0 || day <= maxDay);
        }
    }

    [CreateAssetMenu(fileName = "SupplyRewardTable", menuName = "SO/AirDrop/SupplyRewardTable")]
    public class SupplyRewardTableSO : ScriptableObject
    {
        [SerializeField] private List<SupplyDayRule> dayRules = new();
        [SerializeField] private List<SupplyGuaranteedGroup> guaranteedGroups = new();
        [SerializeField] private List<SupplyRewardEntry> rewardEntries = new();

        public IReadOnlyList<SupplyDayRule> DayRules => dayRules;
        public IReadOnlyList<SupplyGuaranteedGroup> GuaranteedGroups => guaranteedGroups;
        public IReadOnlyList<SupplyRewardEntry> RewardEntries => rewardEntries;

        public SupplyDayRule GetRule(int day)
        {
            if (dayRules == null || dayRules.Count == 0)
                return null;

            SupplyDayRule fallback = null;

            foreach (SupplyDayRule rule in dayRules)
            {
                if (rule == null)
                    continue;

                fallback ??= rule;

                if (rule.ContainsDay(day))
                    return rule;
            }

            return fallback;
        }
    }
}
