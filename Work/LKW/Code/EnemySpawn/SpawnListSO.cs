using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.EnemySpawn
{
    [Serializable]
    public struct SpawnListElement
    {
        public float percentage;
        public List<EnemySO> list;
    }
    [CreateAssetMenu(fileName = "Spawn List", menuName = "SO/EnemySpawn/SpawnList", order = 0)]
    public class SpawnListSO : ScriptableObject
    {
        public SpawnListElement[] daySpawnLists;

        public List<EnemySO> GetSpawnEnemies(int count, int day)
        {
            List<EnemySO> result = new List<EnemySO>();
            if (daySpawnLists == null || daySpawnLists.Length <= 0)
                return result;

            day = Mathf.Clamp(day, 0, daySpawnLists.Length - 1);
            SpawnListElement elem = daySpawnLists[day];
            count = Mathf.FloorToInt(count * Mathf.Clamp01(elem.percentage));
            for (int i = 0; i < count; i++)
            {
                EnemySO enemy = GetEnemy(elem.list);
                if (enemy != null)
                {
                    result.Add(enemy);
                }
            }
            return result;
        }

        public EnemySO GetEnemy(List<EnemySO> targetList)
        {
            int totalWeight = targetList
                .Where(enemy => enemy != null)
                .Sum(enemy => Mathf.Max(0, enemy.spawnRarityWeight));

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"{name} has no positive spawn rarity weight for day.");
                return null;
            }

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var enemy in targetList)
            {
                if (enemy == null || enemy.spawnRarityWeight <= 0)
                {
                    continue;
                }

                currentWeight += enemy.spawnRarityWeight;

                if (randomValue < currentWeight)
                {
                    return enemy;
                }
            }

            return targetList.FirstOrDefault(enemy => enemy != null);
        }
    }
}
