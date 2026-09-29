using System;
using System.Collections.Generic;
using Code.Items;
using Code.Items.ItemInfo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.ETC.MapObjects
{
    [Serializable]
    public class RarityWeight
    {
        public Rarity rarity;
        public float weight;
    }

    [Serializable]
    public class ItemTypeDropConfig
    {
        public ItemType type;
        public List<RarityWeight> rarityWeights;
    }

    public class ItemRandomDropper : ItemDropper
    {
        [SerializeField] private ItemDataBaseSO itemDB;
        [SerializeField] private List<ItemTypeDropConfig> dropConfigs;

        public override PreviewItem Drop(Vector3 from, Vector3 to)
        {
            if (itemDB == null || dropConfigs == null || dropConfigs.Count == 0)
                return null;

            var config = dropConfigs[Random.Range(0, dropConfigs.Count)];
            if (config.rarityWeights == null || config.rarityWeights.Count == 0)
                return null;

            var rarity = GetWeightedRarity(config.rarityWeights);
            var items = itemDB.GetRandomItems(config.type, rarity, 1);
            if (items == null || items.Count == 0)
                return null;

            return SpawnItem(from, to, items[0].CreateItem());
        }

        private static Rarity GetWeightedRarity(List<RarityWeight> rarityWeights)
        {
            float total = 0f;
            foreach (var rarityWeight in rarityWeights)
                total += rarityWeight.weight;

            float roll = Random.Range(0f, total);
            float current = 0f;

            foreach (var rarityWeight in rarityWeights)
            {
                current += rarityWeight.weight;
                if (roll <= current)
                    return rarityWeight.rarity;
            }

            return rarityWeights[rarityWeights.Count - 1].rarity;
        }
    }
}
