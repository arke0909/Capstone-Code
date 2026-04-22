using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Code.EnemySpawn
{
    [CreateAssetMenu(fileName = "Spawn List", menuName = "SO/EnemySpawn/SpawnList", order = 0)]
    public class SpawnListSO : ScriptableObject
    {
        public List<EnemySO> spawnEnemyList;

        public List<EnemySO> GetSpawnEnemies(int count)
        {
            List<EnemySO> result = new List<EnemySO>();

            for (int i = 0; i < count; i++)
            {
                result.Add(GetEnemy());
            }
            return result;
        }

        public EnemySO GetEnemy()
        {
            int totalWeight = spawnEnemyList.Sum(enemy => enemy.spawnRarityWeight);
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var enemy in spawnEnemyList)
            {
                currentWeight += enemy.spawnRarityWeight;

                if (randomValue < currentWeight)
                {
                    return enemy;
                }
            }
            return spawnEnemyList.First();
        }
    }
}