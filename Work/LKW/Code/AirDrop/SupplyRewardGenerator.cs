using System.Collections.Generic;
using Code.Items.ItemInfo;
using UnityEngine;

namespace Code.AirDrop
{
    public class SupplyRewardGenerator
    {
        private readonly Dictionary<SupplyRewardEntry, int> _pickCounts = new();

        public List<SupplyReward> Generate(int currentDay, SupplyRewardTableSO table)
        {
            List<SupplyReward> rewards = new();

            if (table == null)
                return rewards;

            SupplyDayRule rule = table.GetRule(currentDay);
            if (rule == null)
            {
                Debug.LogWarning($"[{nameof(SupplyRewardGenerator)}] No day rule found for day {currentDay}.");
                return rewards;
            }

            _pickCounts.Clear();

            int targetCount = rule.GetRewardCount();
            int remainingBudget = rule.supplyBudget > 0 ? rule.supplyBudget : int.MaxValue;

            AddGuaranteedRewards(table, rule, currentDay, rewards, ref remainingBudget);

            for (int i = 0; i < targetCount; i++)
            {
                Rarity rarity = PickRarity(rule);
                ItemType itemType = PickItemType(rule);

                SupplyRewardEntry entry = PickEntry(table, currentDay, remainingBudget, rarity, itemType);
                if (entry == null)
                    entry = PickEntry(table, currentDay, remainingBudget, Rarity.None, ItemType.None);

                if (entry == null)
                    break;

                int stack = entry.GetStack(rule.stackMultiplier);
                if (stack <= 0)
                    continue;

                rewards.Add(new SupplyReward(entry.itemData, stack));
                remainingBudget -= Mathf.Max(1, entry.cost);

                _pickCounts.TryGetValue(entry, out int count);
                _pickCounts[entry] = count + 1;

                if (remainingBudget <= 0)
                    break;
            }

            _pickCounts.Clear();
            return rewards;
        }

        private void AddGuaranteedRewards(
            SupplyRewardTableSO table,
            SupplyDayRule rule,
            int currentDay,
            List<SupplyReward> rewards,
            ref int remainingBudget)
        {
            bool usedGroup = false;

            foreach (SupplyGuaranteedGroup group in table.GuaranteedGroups)
            {
                if (group == null || !group.IsAvailable(currentDay))
                    continue;

                usedGroup = true;
                for (int i = 0; i < group.pickCount; i++)
                {
                    SupplyRewardEntry entry = PickGuaranteedEntry(group.rewardEntries, currentDay, remainingBudget);
                    if (entry == null)
                    {
                        Debug.LogWarning(
                            $"[{nameof(SupplyRewardGenerator)}] Guaranteed group '{group.groupName}' has no available entry for day {currentDay}.");
                    }

                    AddReward(entry, rule, rewards, ref remainingBudget);
                }
            }

            if (usedGroup)
                return;

            SupplyRewardEntry legacyGuaranteedEntry = PickLegacyGuaranteedEntry(table, currentDay, remainingBudget);
            AddReward(legacyGuaranteedEntry, rule, rewards, ref remainingBudget);
        }

        private void AddReward(
            SupplyRewardEntry entry,
            SupplyDayRule rule,
            List<SupplyReward> rewards,
            ref int remainingBudget)
        {
            if (entry == null)
                return;

            int stack = entry.GetStack(rule.stackMultiplier);
            if (stack <= 0)
                return;

            rewards.Add(new SupplyReward(entry.itemData, stack));
            remainingBudget -= Mathf.Max(1, entry.cost);

            _pickCounts.TryGetValue(entry, out int count);
            _pickCounts[entry] = count + 1;
        }

        private SupplyRewardEntry PickGuaranteedEntry(
            IReadOnlyList<SupplyRewardEntry> entries,
            int currentDay,
            int remainingBudget)
        {
            List<SupplyRewardEntry> candidates = new();

            if (entries == null)
                return null;

            foreach (SupplyRewardEntry entry in entries)
            {
                if (CanPick(entry, currentDay, remainingBudget))
                    candidates.Add(entry);
            }

            return PickWeighted(candidates, entry => entry.weight);
        }

        private SupplyRewardEntry PickLegacyGuaranteedEntry(SupplyRewardTableSO table, int currentDay, int remainingBudget)
        {
            List<SupplyRewardEntry> candidates = new();

            foreach (SupplyRewardEntry entry in table.RewardEntries)
            {
                if (entry != null && entry.guaranteed && CanPick(entry, currentDay, remainingBudget))
                    candidates.Add(entry);
            }

            return PickWeighted(candidates, entry => entry.weight);
        }

        private SupplyRewardEntry PickEntry(
            SupplyRewardTableSO table,
            int currentDay,
            int remainingBudget,
            Rarity rarity,
            ItemType itemType)
        {
            List<SupplyRewardEntry> candidates = new();

            foreach (SupplyRewardEntry entry in table.RewardEntries)
            {
                if (!CanPick(entry, currentDay, remainingBudget))
                    continue;

                if (rarity != Rarity.None && entry.Rarity != rarity)
                    continue;

                if (itemType != ItemType.None && entry.ItemType != itemType)
                    continue;

                candidates.Add(entry);
            }

            return PickWeighted(candidates, entry => entry.weight);
        }

        private bool CanPick(SupplyRewardEntry entry, int currentDay, int remainingBudget)
        {
            if (entry == null || !entry.IsAvailable(currentDay))
                return false;

            if (entry.cost > remainingBudget)
                return false;

            if (entry.maxPickCount <= 0)
                return true;

            _pickCounts.TryGetValue(entry, out int count);
            return count < entry.maxPickCount;
        }

        private Rarity PickRarity(SupplyDayRule rule)
        {
            SupplyRarityWeight weight = PickWeighted(rule.rarityWeights, item => item.weight);
            return weight != null ? weight.rarity : Rarity.None;
        }

        private ItemType PickItemType(SupplyDayRule rule)
        {
            SupplyItemTypeWeight weight = PickWeighted(rule.itemTypeWeights, item => item.weight);
            return weight != null ? weight.itemType : ItemType.None;
        }

        private static T PickWeighted<T>(IReadOnlyList<T> candidates, System.Func<T, float> getWeight) where T : class
        {
            if (candidates == null || candidates.Count == 0)
                return null;

            float totalWeight = 0f;
            foreach (T candidate in candidates)
            {
                totalWeight += Mathf.Max(0f, getWeight(candidate));
            }

            if (totalWeight <= 0f)
                return candidates[Random.Range(0, candidates.Count)];

            float roll = Random.Range(0f, totalWeight);
            float current = 0f;

            foreach (T candidate in candidates)
            {
                current += Mathf.Max(0f, getWeight(candidate));
                if (roll <= current)
                    return candidate;
            }

            return candidates[^1];
        }
    }
}
