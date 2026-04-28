using System.Collections.Generic;
using Code.SHS.Entities.Enemies;
using Code.TimeSystem;
using UnityEngine;

namespace Code.EnemySpawn
{
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private List<EnemySO> spawnEnemies;
        [SerializeField] private SpawnListSO spawnList;

        private void Start()
        {
            foreach (Transform child in transform)
            {
                spawnPoints.Add(child);
            }

            SetUpSpawnZone();
            
            TimeController.Instance.AddRepeatEvent(TimeUtil.Day(0.5f), SpawnAllEnemies);
        }

        private void SetUpSpawnZone()
        {
            if (spawnPoints == null || spawnPoints.Count <= 0)
                return;

            spawnEnemies = spawnList.GetSpawnEnemies(spawnPoints.Count);

            SpawnAllEnemies();
        }

        public void SpawnAllEnemies()
        {
            if (spawnPoints == null || spawnEnemies == null) return;


            for (int i = 0; i < spawnPoints.Count; i++)
            {
                int randomIndex = Random.Range(0, spawnEnemies.Count);

                SpawnEnemy(spawnEnemies[randomIndex], spawnPoints[i].position, spawnPoints[i].rotation);
            }
        }

        public void SpawnEnemy(EnemySO enemyData, Vector3 position, Quaternion rotation)
        {
            if (enemyData == null || enemyData.enemyPrefab == null) return;

            EnemySpawnUtility.SpawnEnemy(enemyData, position, rotation);
        }
    }
}
