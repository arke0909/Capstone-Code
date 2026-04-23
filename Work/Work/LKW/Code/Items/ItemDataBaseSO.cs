using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;
using Random = UnityEngine.Random;

namespace Work.LKW.Code.Items
{
    [CreateAssetMenu(fileName = "Item DB", menuName = "SO/Items/ItemDataBase")]
    public class 
        ItemDataBaseSO : ScriptableObject
    {
        public List<ItemDataSO> allItems;

        private Dictionary<ItemType, List<ItemDataSO>> itemDataByType;
        private Dictionary<Rarity, List<ItemDataSO>> itemDataByRarity;
        private Dictionary<SpawnArea, List<ItemDataSO>> itemDataBySpawnArea;

        private void OnEnable()
        {
            itemDataByType = allItems.GroupBy(item => item.itemType)
                .ToDictionary(group => group.Key, group => group.ToList());

            itemDataByRarity = allItems.GroupBy(item => item.rarity)
                .ToDictionary(group => group.Key, group => group.ToList());

            itemDataBySpawnArea = allItems.GroupBy(item => item.spawnArea)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        public List<ItemDataSO> GetItemList(ItemType itemType, Rarity rarity)
        {
            // 같은 레어도에 카테고리 아이템 리스트 일괄 반환
            return itemDataByType[itemType].Where(data => data.rarity == rarity).ToList();
        }

        //상자중에 모든 등급이 포함된 특정 아이템 상자등이 나오면 필요함
        public List<ItemDataSO> GetItemByType(ItemType itemType)
        {
            return itemDataByType.GetValueOrDefault(itemType);
        }

        // 상자 중에 특정등급에 무작위 카테고리 아이템이 나오는 상자에 필요함
        public List<ItemDataSO> GetItemByRarity(Rarity rarity)
            => itemDataByRarity[rarity];

        public List<ItemDataSO> GetItemBySpawnArea(SpawnArea area)
            => itemDataBySpawnArea[area];


        // 가중치에 따라 랜덤으로 하나 쁩는
        public ItemDataSO GetRandomItem(List<ItemDataSO> items)
        {
            int totalWeight = items.Sum(item => item.rarityWeight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var item in items)
            {
                currentWeight += item.rarityWeight;

                if (randomValue < currentWeight)
                {
                    return item;
                }
            }
            return items.First();
        }


        // 같은 타입에 아이템 랜덤 못록 반환함
        public List<ItemDataSO> GetRandomItems(ItemType type, int count)
        {
            List<ItemDataSO> targetItems = GetItemByType(type);

            List<ItemDataSO> result = new List<ItemDataSO>();

            for (int i = 0; i < count; i++)
            {
                result.Add(GetRandomItem(targetItems));
            }

            return result;
        }


        // 이건 위랑 같은데 희귀도 기준
        public List<ItemDataSO> GetRandomItems(Rarity rarity, int count)
        {
            List<ItemDataSO> targetItems = GetItemByRarity(rarity);

            List<ItemDataSO> result = new List<ItemDataSO>();

            for (int i = 0; i < count; i++)
            {
                result.Add(GetRandomItem(targetItems));
            }

            return result;
        }


        public List<ItemDataSO> GetRandomItems(List<ItemDataSO> targetItems, SpawnArea area, int count)
        {
            var filtered = targetItems.Where(i => (i.spawnArea & area) != 0).ToList();
            if (filtered.Count == 0)
            {
                Debug.LogError($"No items type : {targetItems.First().GetType()} for SpawnArea {area}");
                return new List<ItemDataSO>();
            }

            List<ItemDataSO> result = new List<ItemDataSO>();

            for (int i = 0; i < count; i++) 
            {
                result.Add(GetRandomItem(filtered));
            }

            return result;
        }
    }
}