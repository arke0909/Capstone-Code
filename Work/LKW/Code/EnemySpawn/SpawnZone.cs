using Code.SHS.Entities.Enemies;
using Code.TimeSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Code.EnemySpawn
{
    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private SpawnListSO spawnList;

        private readonly List<Enemy> spawnedEnemies = new();
        private readonly Dictionary<Enemy, UnityAction> enemyDeadCallbacks = new();

        private void Start()
        {
            spawnPoints ??= new List<Transform>();
            foreach (Transform child in transform)
            {
                spawnPoints.Add(child);
            }

            SetUpSpawnZone();

            TimeController.Instance.AddRepeatEvent(720, SpawnAllEnemies);
        }

        private void SetUpSpawnZone()
        {
            if (spawnPoints == null || spawnPoints.Count <= 0)
                return;

            SpawnAllEnemies();
        }

        public void SpawnAllEnemies()
        {
            ClearSpawnedEnemies();

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

            Enemy spawnedEnemy = EnemySpawnUtility.SpawnEnemy(enemyData, position, rotation);
            RegisterSpawnedEnemy(spawnedEnemy);
        }

        private void RegisterSpawnedEnemy(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (!spawnedEnemies.Contains(enemy))
                spawnedEnemies.Add(enemy);

            if (enemyDeadCallbacks.ContainsKey(enemy))
                return;

            UnityAction deadCallback = () => RemoveSpawnedEnemy(enemy);
            enemyDeadCallbacks.Add(enemy, deadCallback);
            enemy.OnDeadEvent.AddListener(deadCallback);
        }

        private void RemoveSpawnedEnemy(Enemy enemy)
        {
            if (enemy == null)
                return;

            if (enemyDeadCallbacks.TryGetValue(enemy, out UnityAction deadCallback))
            {
                enemy.OnDeadEvent.RemoveListener(deadCallback);
                enemyDeadCallbacks.Remove(enemy);
            }

            spawnedEnemies.Remove(enemy);
        }

        private void ClearSpawnedEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = spawnedEnemies[i];
                if (enemy == null)
                    continue;

                RemoveSpawnedEnemy(enemy);

                enemy.ReleaseToPool();
            }

            spawnedEnemies.Clear();
        }
    }
}
