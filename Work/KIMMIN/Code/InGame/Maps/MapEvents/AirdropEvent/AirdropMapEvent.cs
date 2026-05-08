using System;
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
    public class AirdropMapEvent : DropStructureEvent
    {
        [SerializeField] private PoolItemSO airdropPool;
        [SerializeField] private List<EnemySO> enemies;

        [SerializeField] private int dropCount = 1;
        [SerializeField] private int enemyCount = 5;

        [Inject] private PoolManagerMono _poolManager;
        private readonly float _height = 100f;

        protected override void StartDropStructureEvent()
        {
            int length = AreaCount;
            int count = Mathf.Min(dropCount, length);

            Span<int> indices = stackalloc int[length];
            FillShuffledAreaIndices(indices);

            for (int i = 0; i < count; i++)
            {
                InitAirdropEvent(indices[i]);
            }
        }

        private void InitAirdropEvent(int areaIdx)
        {
            if (!TryGetRandomAreaPoint(areaIdx, out AreaPoint areaPoint))
                return;

            Vector3 position = areaPoint.Position;

            Airdrop airdrop = RegisterDropStructure(_poolManager.Pop<Airdrop>(airdropPool));
            airdrop.StartDrop(position, _height, HandleLandning);
            SpawnEnemies(position);

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
    }
}
