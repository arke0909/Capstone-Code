using System.Collections.Generic;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.PlayerTasks.Tasks
{
    public class CombatTask : PlayerTask, IProgressTask
    {
        [SerializeField] private EnemySO spawnEnemy;
        [SerializeField] private Transform[] spawnPoints;
        
        private int _currentEnemyCount;
        private List<Enemy> _enemies = new();
        private int _enemyCount;

        public bool HasProgress => _enemyCount > 0;

        public float Progress
        {
            get
            {
                if (_enemyCount <= 0)
                    return 0f;

                return (float)_currentEnemyCount / _enemyCount;
            }
        }

        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            _enemyCount = spawnPoints.Length;
        }

        public override void StartTask()
        {
            UpdateTaskText();
            SpawnEnemies();
        }
        
        private void SpawnEnemies()
        {
            for (int i = 0; i < _enemyCount; i++)
            {
                if (spawnEnemy == null || spawnEnemy.enemyPrefab == null)
                    return;

                GameObject go = Instantiate(spawnEnemy.enemyPrefab, 
                    spawnPoints[i].position, Quaternion.identity);
                
                Enemy enemy = go.GetComponent<Enemy>();
                enemy.SpawnEnemy(spawnPoints[i].position, spawnEnemy);
                enemy.OnDeadEvent.AddListener(HandleEnemyDead);
                
                _enemies.Add(enemy);
            }
        }
        
        private void HandleEnemyDead()
        {
            _currentEnemyCount++;
            UpdateTaskText();
            
            if (_currentEnemyCount >= _enemyCount)
            {
                CompleteTask();
            }
        }

        protected override void StopTask()
        {
            foreach (Enemy enemy in _enemies)
            {
                enemy.OnDeadEvent.RemoveListener(HandleEnemyDead);
            }
            
            _enemies.Clear();
        }

        protected override string GetTaskText()
        {
            return $"적을 처지하세요({_currentEnemyCount}/{_enemyCount})";
        }
    }
}
