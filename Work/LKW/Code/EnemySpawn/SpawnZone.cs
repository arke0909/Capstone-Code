using System.Collections.Generic;
using Code.SHS.Entities.Enemies;
using Code.TimeSystem;
using UnityEngine;

namespace Code.EnemySpawn
{
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
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

            SpawnAllEnemies();
        }

        public void SpawnAllEnemies()
        {
            if (spawnPoints == null || spawnList == null) return;

            int currentDay = TimeController.Instance.CurrentDay;
            List<EnemySO> spawnEnemies = spawnList.GetSpawnEnemies(spawnPoints.Count, currentDay);

            if (spawnEnemies == null || spawnEnemies.Count <= 0) return;

            List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
            int spawnCount = Mathf.Min(spawnEnemies.Count, availableSpawnPoints.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int spawnPointIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                availableSpawnPoints.RemoveAt(spawnPointIndex);

                SpawnEnemy(spawnEnemies[i], spawnPoint.position, spawnPoint.rotation);
            }
        }

        public void SpawnEnemy(EnemySO enemyData, Vector3 position, Quaternion rotation)
        {
            if (enemyData == null || enemyData.enemyPrefab == null) return;

            EnemySpawnUtility.SpawnEnemy(enemyData, position, rotation);
        }
    }
}
