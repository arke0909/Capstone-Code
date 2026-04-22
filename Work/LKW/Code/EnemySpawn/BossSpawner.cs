using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Chipmunk.GameEvents;
using Code.SHS.Entities.Enemies;
using Scripts.Combat.Areas;
using Sirenix.OdinInspector;
using UnityEngine;
using Work.Code.GameEvents;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.EnemySpawn
{
    public class BossSpawner : SerializedMonoBehaviour
    {
        public Dictionary<int, EnemySO> BossDict;
        
        private void Start()
        {
            Bus.Subscribe<AirdropEvent>(HandleBossSpawn);
        }

        private void OnDestroy()
        {
            Bus.Unsubscribe<AirdropEvent>(HandleBossSpawn);
        }

        private void HandleBossSpawn(AirdropEvent evt)
        {
            EnemySO bossData = BossDict[evt.Area];

            Vector3 spawnPos = evt.Position;

            GameObject enemyObject = Instantiate(bossData.enemyPrefab, spawnPos, Quaternion.identity);

            Enemy enemy = enemyObject.GetComponent<Enemy>();
            enemy.SpawnEnemy(spawnPos,bossData);
        }
    }
}