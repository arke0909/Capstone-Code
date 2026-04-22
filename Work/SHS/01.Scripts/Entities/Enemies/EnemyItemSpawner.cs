using Code.EnemySpawn;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public class EnemyItemSpawner : MonoBehaviour
    {
        [SerializeField] private Enemy enemy;
        [SerializeField] private EnemySO enemyData;

        private void Reset()
        {
            enemy = GetComponentInChildren<Enemy>();
        }

        private void Start()
        {
            enemy.SpawnEnemy(enemy.transform.position,enemyData);
        }
    }
}