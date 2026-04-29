using System.Collections.Generic;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class CombatTutorialState : TutorialState
    {
        [SerializeField] private EnemySO spawnEnemy;
        [SerializeField] private Transform[] spawnPoints;
        
        [Inject] private PoolManagerMono _poolManager;
        
        private int _currentEnemyCount;
        private readonly int _enemyCount = 3;
        private List<Enemy> _enemies = new();

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            
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

        public override void ExitTutorial()
        {
            foreach (Enemy enemy in _enemies)
            {
                enemy.OnDeadEvent.RemoveListener(HandleEnemyDead);
            }
        }

        private void HandleEnemyDead()
        {
            _currentEnemyCount++;
            _tutorialController.SetDialogue(GetDialogue(), true);
        }

        protected override string GetDialogue()
        {
            return $"{tutorialStateSO.dialogue}({_currentEnemyCount}/{_enemyCount})";
        }
    }
}