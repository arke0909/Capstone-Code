using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Events.Local;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using Work.Code.GameEvents;
using Work.Code.MapEvents.Elements;
using Random = UnityEngine.Random;

namespace Work.Code.MapEvents
{
    public class AirdropMapEvent : MapEvent
    {
        [SerializeField] private Transform[] roots;
        [SerializeField] private PoolItemSO airdropPool;
        [SerializeField] private List<EnemySO> enemies;
        [SerializeField] private int dropCount = 1;
        [SerializeField] private int enemyCount = 5;
        
        [Inject] private PoolManagerMono _poolManager;
        private Queue<Airdrop> _airdrops = new();
        
        protected override void StartEvent()
        {
            int length = roots.Length;
            int count = Mathf.Min(dropCount, length);

            List<int> indices = new List<int>(length);
            for (int i = 0; i < length; i++)
            {
                indices.Add(i);
            }

            for (int i = 0; i < length; i++)
            {
                int rand = Random.Range(i, length);
                (indices[i], indices[rand]) = (indices[rand], indices[i]);
            }

            for (int i = 0; i < count; i++)
            {
                if (_airdrops.Count > 0)
                {
                    var airdrop = _airdrops.Dequeue();
                    airdrop.TakeAirdrop();
                }
                
                StartAirdrop(indices[i]);
            }
        }

        private void StartAirdrop(int area)
        {
            int point = Random.Range(0, roots[area].childCount);
            var airdrop = _poolManager.Pop<Airdrop>(airdropPool);
            Vector3 position = roots[area].GetChild(point).position;
            
            airdrop.StartDrop(position, 100f, HandleLadning);
            SpawnEnemies(position);
            
            _airdrops.Enqueue(airdrop);
            EventName = $"{area + 1}지역 보급 낙하!";
            EventBus.Raise(new AirdropEvent(area, position));
        }

        private void HandleLadning(Vector3 landingPos)
        {
            landingPos.y = 0;
            SpawnEnemies(landingPos);
        }


        private void SpawnEnemies(Vector3 position)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 pos = position + GetRandomPosition();
                EnemySO enemy = enemies[Random.Range(0, enemies.Count)];
                Enemy newEnemy = Instantiate(enemy.enemyPrefab, pos, Quaternion.identity)
                    .GetComponent<Enemy>();
                newEnemy.SpawnEnemy(pos, enemy);
            }
        }

        private Vector3 GetRandomPosition()
        {
            Vector2 circle = Random.insideUnitCircle * Random.Range(2f, 5f);
            return new Vector3(circle.x, 0f, circle.y);
        }
        
        private void OnValidate()
        {
            for(int i = 0; i < roots.Length; i++)
            {
                foreach(Transform trm in roots[i])
                {
                    trm.name = $"Area{i + 1}_Pos{trm.GetSiblingIndex() + 1}";
                }
            }
        }
    }
}