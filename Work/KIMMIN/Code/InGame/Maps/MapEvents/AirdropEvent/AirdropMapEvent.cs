using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using Work.Code.Core.Extension;
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
        private readonly Queue<Airdrop> _airdropQueue = new();
        private readonly float _height = 100f;
        
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
                if (_airdropQueue.Count > 0)
                {
                    var airdrop = _airdropQueue.Dequeue();
                    airdrop.TakeAirdrop();
                }
                
                InitAirdropEvent(indices[i]);
            }
        }

        private void InitAirdropEvent(int areaIdx)
        {
            int point = Random.Range(0, roots[areaIdx].childCount);
            Vector3 position = roots[areaIdx].GetChild(point).position;
            
            Airdrop airdrop = _poolManager.Pop<Airdrop>(airdropPool);
            airdrop.StartDrop(position, _height, HandleLandning);
            SpawnEnemies(position);
            
            _airdropQueue.Enqueue(airdrop);
            EventName = $"{areaIdx + 1}지역 보급 낙하!";
            EventBus.Raise(new AirdropEvent(areaIdx, position));
        }

        private void HandleLandning(Vector3 landingPos)
        {
            landingPos.y = 0;
            SpawnEnemies(landingPos);
        }

        private void SpawnEnemies(Vector3 position)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPos = position.GetRandomInsideUnitCircle(2f, 5f);
                EnemySO enemy = enemies[Random.Range(0, enemies.Count)];
                EnemySpawnUtility.SpawnEnemy(enemy, spawnPos, Quaternion.identity, _poolManager);
            }
        }

#if UNITY_EDITOR
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
#endif
    }
}
