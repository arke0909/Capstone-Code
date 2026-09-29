using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.AirDrop;
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
        [SerializeField] private SupplySpawnTableSO supplySpawnTable;

        [SerializeField] private int dropCount = 1;
        [SerializeField] private int enemyCount = 5;

        [Inject] private PoolManagerMono _poolManager;
        private readonly float _height = 100f;

        protected override void StartDropStructureEvent()
        {
            if (TryStartSupplyTableDrops())
                return;

            StartLegacyDrops();
        }

        private bool TryStartSupplyTableDrops()
        {
            if (supplySpawnTable == null)
                return false;

            int currentDay = _timeController != null ? Mathf.Max(1, _timeController.CurrentDay) : 1;
            IReadOnlyList<SupplyDropDefinition> drops = supplySpawnTable.GetDrops(currentDay);
            if (drops == null || drops.Count <= 0)
            {
                Debug.LogWarning($"[{nameof(AirdropMapEvent)}] No supply drops found for day {currentDay}.", this);
                return false;
            }

            int length = AreaCount;
            int count = Mathf.Min(drops.Count, length);
            if (count <= 0)
            {
                Debug.LogWarning($"[{nameof(AirdropMapEvent)}] No airdrop areas are assigned.", this);
                return true;
            }

            if (count < drops.Count)
            {
                Debug.LogWarning(
                    $"[{nameof(AirdropMapEvent)}] Not enough airdrop areas. Requested {drops.Count}, spawned {count}.",
                    this);
            }

            Span<int> indices = stackalloc int[length];
            FillShuffledAreaIndices(indices);

            for (int i = 0; i < count; i++)
            {
                InitAirdropEvent(indices[i], drops[i]);
            }

            return true;
        }

        private void StartLegacyDrops()
        {
            int length = AreaCount;
            int count = Mathf.Min(dropCount, length);

            Span<int> indices = stackalloc int[length];
            FillShuffledAreaIndices(indices);

            for (int i = 0; i < count; i++)
            {
                InitAirdropEvent(indices[i], null);
            }
        }

        private void InitAirdropEvent(int areaIdx, SupplyDropDefinition dropDefinition)
        {
            if (!TryGetRandomAreaPoint(areaIdx, out AreaPoint areaPoint))
                return;

            Vector3 position = areaPoint.Position;
            SpawnEnemies(position);
            position.y = _height;
            PoolItemSO selectedAirdropPool = GetAirdropPool(dropDefinition);
            if (selectedAirdropPool == null)
            {
                Debug.LogWarning($"[{nameof(AirdropMapEvent)}] Airdrop pool is missing.", this);
                return;
            }

            Airdrop airdrop = _poolManager.Pop<Airdrop>(selectedAirdropPool);
            if (airdrop == null)
            {
                string supplyNameForLog = dropDefinition != null ? dropDefinition.DisplayName : "Supply";
                Debug.LogWarning(
                    $"[{nameof(AirdropMapEvent)}] Failed to pop airdrop for '{supplyNameForLog}'. Check PoolManagerSO itemList.",
                    this);
                return;
            }

            RegisterDropStructure(airdrop);
            airdrop.StartDrop(position, dropDefinition?.rewardTable, dropDefinition?.minimapIcon, HandleLandning);

            string supplyName = dropDefinition != null ? dropDefinition.DisplayName : "보급";
            EventName = $"{areaIdx + 1}지역 {supplyName} 낙하!";
            EventBus.Raise(new MapEventStartEvent(this, MapEventSO.duration));
            EventBus.Raise(new AirdropEvent(areaIdx, position, airdrop.Inventory));
        }

        private PoolItemSO GetAirdropPool(SupplyDropDefinition dropDefinition)
        {
            if (dropDefinition != null && dropDefinition.airdropPool != null)
                return dropDefinition.airdropPool;

            return airdropPool;
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
        protected override void OnEventCalled()
        {
            StartEvent();
        }
    }
}
