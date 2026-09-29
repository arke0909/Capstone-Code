using Code.SHS.Entities.Enemies;
using Code.StatusEffectSystem;
using Code.TimeSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Code.EnemySpawn
{
    [Serializable]
    public class FixedEnemySpawnEntry
    {
        public bool isEnabled = true;
        public EnemySO enemy;
        public Transform spawnPoint;
    }

    public class SpawnZone : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private SpawnListSO spawnList;
        [SerializeField] private List<FixedEnemySpawnEntry> fixedSpawns = new();
        [SerializeField] private BuffSO initBuff;

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
            if ((spawnPoints == null || spawnPoints.Count <= 0) &&
                (fixedSpawns == null || fixedSpawns.Count <= 0))
                return;

            SpawnAllEnemies();
        }

        public void SpawnAllEnemies()
        {
            ClearSpawnedEnemies();

            HashSet<Transform> reservedSpawnPoints = SpawnFixedEnemies();
            SpawnRandomEnemies(reservedSpawnPoints);
        }

        private HashSet<Transform> SpawnFixedEnemies()
        {
            HashSet<Transform> reservedSpawnPoints = new();
            if (fixedSpawns == null)
                return reservedSpawnPoints;

            foreach (FixedEnemySpawnEntry fixedSpawn in fixedSpawns)
            {
                if (fixedSpawn == null || !fixedSpawn.isEnabled)
                    continue;
                if (fixedSpawn.enemy == null || fixedSpawn.spawnPoint == null)
                    continue;

                reservedSpawnPoints.Add(fixedSpawn.spawnPoint);
                SpawnEnemy(fixedSpawn.enemy, fixedSpawn.spawnPoint.position, fixedSpawn.spawnPoint.rotation);
            }

            return reservedSpawnPoints;
        }

        private void SpawnRandomEnemies(HashSet<Transform> reservedSpawnPoints)
        {
            if (spawnPoints == null || spawnList == null) return;

            List<Transform> availableSpawnPoints = GetAvailableRandomSpawnPoints(reservedSpawnPoints);
            if (availableSpawnPoints.Count <= 0) return;

            int currentDay = TimeController.Instance.CurrentDay;
            List<EnemySO> spawnEnemies = spawnList.GetSpawnEnemies(availableSpawnPoints.Count, currentDay);

            if (spawnEnemies == null || spawnEnemies.Count <= 0) return;

            int spawnCount = Mathf.Min(spawnEnemies.Count, availableSpawnPoints.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int spawnPointIndex = UnityEngine.Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                availableSpawnPoints.RemoveAt(spawnPointIndex);

                SpawnEnemy(spawnEnemies[i], spawnPoint.position, spawnPoint.rotation);
            }
        }

        private List<Transform> GetAvailableRandomSpawnPoints(HashSet<Transform> reservedSpawnPoints)
        {
            List<Transform> availableSpawnPoints = new();
            HashSet<Transform> addedSpawnPoints = new();

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                    continue;
                if (reservedSpawnPoints != null && reservedSpawnPoints.Contains(spawnPoint))
                    continue;
                if (!addedSpawnPoints.Add(spawnPoint))
                    continue;

                availableSpawnPoints.Add(spawnPoint);
            }

            return availableSpawnPoints;
        }

        public void SpawnEnemy(EnemySO enemyData, Vector3 position, Quaternion rotation)
        {
            if (enemyData == null || enemyData.enemyPrefab == null) return;

            Enemy spawnedEnemy = EnemySpawnUtility.SpawnEnemy(enemyData, position, rotation,null,initBuff);
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
