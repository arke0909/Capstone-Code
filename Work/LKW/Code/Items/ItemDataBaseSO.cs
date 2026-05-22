using System.Collections.Generic;
using System.Linq;
using Code.Items.ItemInfo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Items
{
    [CreateAssetMenu(fileName = "Item DB", menuName = "SO/Items/ItemDataBase")]
    public class ItemDataBaseSO : ScriptableObject
    {
        public List<ItemDataSO> allItems;

        private Dictionary<ItemType, List<ItemDataSO>> _itemDataByType;
        private Dictionary<Rarity, List<ItemDataSO>> _itemDataByRarity;
        private Dictionary<SpawnArea, List<ItemDataSO>> _itemDataBySpawnArea;

        private void OnEnable() => Initialize();

        private void Initialize()
        {
            Debug.Log($"[ItemDB] Initialize - allItems count: {allItems?.Count ?? -1}");
            _itemDataByType = allItems.GroupBy(item => item.itemType)
                .ToDictionary(group => group.Key, group => group.ToList());

            _itemDataByRarity = allItems.GroupBy(item => item.rarity)
                .ToDictionary(group => group.Key, group => group.ToList());

            _itemDataBySpawnArea = allItems.GroupBy(item => item.spawnArea)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        public List<ItemDataSO> GetItemsByType(ItemType itemType)
        {
            if (_itemDataByType == null) Initialize();
            return _itemDataByType.GetValueOrDefault(itemType) ?? new List<ItemDataSO>();
        }

        public List<ItemDataSO> GetItemsByRarity(Rarity rarity)
        {
            if (_itemDataByRarity == null) Initialize();
            return _itemDataByRarity.GetValueOrDefault(rarity) ?? new List<ItemDataSO>();
        }

        private ItemDataSO GetRandomItem(List<ItemDataSO> items)
        {
            if (items == null || items.Count == 0)
            {
                Debug.LogError("[ItemDB] GetRandomItem - 아이템 목록이 비어있음");
                return null;
            }

            int totalWeight = items.Sum(item => item.rarityWeight);
            if (totalWeight <= 0)
            {
                Debug.LogError("[ItemDB] GetRandomItem - totalWeight가 0 이하");
                return items[Random.Range(0, items.Count)];
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var item in items)
            {
                currentWeight += item.rarityWeight;
                if (randomValue < currentWeight)
                    return item;
            }

            return items[^1];
        }

        private List<ItemDataSO> PickRandomItems(List<ItemDataSO> targetItems, int count)
        {
            if (targetItems == null || targetItems.Count == 0)
            {
                Debug.LogError("[ItemDB] PickRandomItems - 아이템 목록이 비어있음");
                return new List<ItemDataSO>();
            }

            List<ItemDataSO> result = new List<ItemDataSO>();
            for (int i = 0; i < count; i++)
            {
                var item = GetRandomItem(targetItems);
                if (item != null) result.Add(item);
            }
            return result;
        }

        public List<ItemDataSO> GetRandomItems(ItemType type, int count)
            => PickRandomItems(GetItemsByType(type), count);

        public List<ItemDataSO> GetRandomItems(Rarity rarity, int count)
            => PickRandomItems(GetItemsByRarity(rarity), count);

        public List<ItemDataSO> GetRandomItems(ItemType type, Rarity rarity, int count)
        {
            var filtered = GetItemsByType(type)
                .Where(item => item.rarity == rarity)
                .ToList();

            if (filtered.Count == 0)
            {
                Debug.LogError($"[ItemDB] GetRandomItems - Type:{type}, Rarity:{rarity} 조합의 아이템 없음. DB 등록 확인 필요.");
                return new List<ItemDataSO>();
            }
            return PickRandomItems(filtered, count);
        }

        public List<ItemDataSO> GetRandomItems(List<ItemDataSO> targetItems, SpawnArea area, int count)
        {
            var filtered = targetItems.Where(i => (i.spawnArea & area) != 0).ToList();
            if (filtered.Count == 0)
            {
                Debug.LogError($"[ItemDB] SpawnArea {area} 에 해당하는 아이템 없음");
                return new List<ItemDataSO>();
            }
            return PickRandomItems(filtered, count);
        }
    }
}