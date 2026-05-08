using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.EnemySpawn
{
    [Serializable]
    public class DaySpawnList
    {
        public int day;
        public List<EnemySO> spawnEnemyList;
    }

    [CreateAssetMenu(fileName = "Spawn List", menuName = "SO/EnemySpawn/SpawnList", order = 0)]
    public class SpawnListSO : ScriptableObject
    {
        public List<EnemySO> spawnEnemyList;
        public List<DaySpawnList> daySpawnLists;

        public List<EnemySO> GetSpawnEnemies(int count)
        {
            return GetSpawnEnemies(count, 0);
        }

        public List<EnemySO> GetSpawnEnemies(int count, int day)
        {
            List<EnemySO> result = new List<EnemySO>();

            for (int i = 0; i < count; i++)
            {
                EnemySO enemy = GetEnemy(day);
                if (enemy != null)
                {
                    result.Add(enemy);
                }
            }
            return result;
        }

        public EnemySO GetEnemy()
        {
            return GetEnemy(0);
        }

        public EnemySO GetEnemy(int day)
        {
            List<EnemySO> targetList = GetSpawnEnemyList(day);
            if (targetList == null || targetList.Count <= 0)
            {
                Debug.LogWarning($"{name} has no enemy spawn list for day {day}.");
                return null;
            }

            int totalWeight = targetList
                .Where(enemy => enemy != null)
                .Sum(enemy => Mathf.Max(0, enemy.spawnRarityWeight));

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"{name} has no positive spawn rarity weight for day {day}.");
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

        private List<EnemySO> GetSpawnEnemyList(int day)
        {
            DaySpawnList matchedList = null;
            bool hasDuplicate = false;

            if (daySpawnLists != null)
            {
                foreach (DaySpawnList daySpawnList in daySpawnLists)
                {
                    if (daySpawnList == null || daySpawnList.day != day)
                    {
                        continue;
                    }

                    if (matchedList == null && daySpawnList.spawnEnemyList != null && daySpawnList.spawnEnemyList.Count > 0)
                    {
                        matchedList = daySpawnList;
                        continue;
                    }

                    hasDuplicate = true;
                }
            }

            if (hasDuplicate)
            {
                Debug.LogWarning($"{name} has duplicate day spawn lists for day {day}. The first valid list will be used.");
            }

            if (matchedList != null)
            {
                return matchedList.spawnEnemyList;
            }

            return spawnEnemyList;
        }
    }
}
