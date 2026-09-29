using Chipmunk.ComponentContainers;
using Code.ItemContainers;
using Code.Items;
using Code.Items.ItemInfo;
using Code.SHS.Entities.Enemies;
using Code.TimeSystem;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Code.EnemySpawn
{
    public class BossSpawner : MonoBehaviour
    {
        [field: SerializeField] public EnemySO bossSO { get; private set; }
        [field: SerializeField] public string bossName { get; private set; }
        [SerializeField] private Transform targetTransform;
        [SerializeField] private ItemContainer rewardContainer;
        [SerializeField] private List<ItemDataSO> rewardItems = new();
        [SerializeField] private ItemDataBaseSO itemDB;
        [SerializeField] private float respawnDelayHours = 12f;
        [SerializeField] private GameObject outDoor;
        [Inject] private PoolManagerMono _poolManager;
        private Enemy _currentEnemy;
        private float _beforeTimeScale;
        private bool _isPlayerInside;

        public float RemainingRespawnHours { get; private set; }

        private void Start()
        {
            rewardContainer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _currentEnemy?.OnDeadEvent.RemoveListener(HandleBossDead);
        }

        [ContextMenu("Spawn")]
        private void Spawn()
        {
            EnsureBossSpawned();
        }

        public bool TryEnter(Entity entity)
        {
            return EnterInternal(entity);
        }

        public void Enter(Entity entity)
        {
            EnterInternal(entity);
        }

        private bool EnterInternal(Entity entity)
        {
            if (_isPlayerInside)
                return false;

            if (!EnsureBossSpawned())
                return false;
            _beforeTimeScale = TimeController.Instance.TimeScale;
            TimeController.Instance.TimeScale = 0;
            rewardContainer?.gameObject.SetActive(false);
            outDoor.SetActive(false);
            _isPlayerInside = true;
            return true;
        }

        private void HandleBossDead()
        {
            Debug.Log("qoiwdhj9uiwqodvhuifwjhvuiydwv");
            _currentEnemy?.OnDeadEvent.RemoveListener(HandleBossDead);
            _currentEnemy = null;
            outDoor.SetActive(true);
            SpawnRewardContainer();
        }

        private bool EnsureBossSpawned()
        {
            if (_currentEnemy != null && !_currentEnemy.IsDead)
            {
                _currentEnemy.transform.SetPositionAndRotation(targetTransform.position, Quaternion.identity);
                _currentEnemy.OnDeadEvent.RemoveListener(HandleBossDead);
                _currentEnemy.OnDeadEvent.AddListener(HandleBossDead);
                return true;
            }

            _currentEnemy = EnemySpawnUtility.SpawnEnemy(bossSO, targetTransform.position, Quaternion.identity, _poolManager);
            if (_currentEnemy == null)
                return false;

            _currentEnemy.OnDeadEvent.AddListener(HandleBossDead);
            return true;
        }

        private void SpawnRewardContainer()
        {
            rewardContainer.gameObject.SetActive(true);
            Debug.Log("Reawasdasd");
            SetUpRewardContainer();
        }

        private void SetUpRewardContainer()
        {
            if (rewardItems != null && rewardItems.Count > 0)
            {
                rewardContainer.Inventory.SetUpItem(rewardItems);
                return;
            }

            if (itemDB == null)
                return;

            List<ItemDataSO> targetItems = GetTargetRewardItems();
            if (targetItems.Count == 0)
                return;

            int count = rewardContainer.GetRandomCount();
            List<ItemDataSO> resultItems = rewardContainer.AllowedSpawnArea == SpawnArea.None
                ? GetRandomRewardItems(targetItems, count)
                : itemDB.GetRandomItems(targetItems, rewardContainer.AllowedSpawnArea, count);
            if (resultItems.Count > 0)
                rewardContainer.Inventory.SetUpItem(resultItems);
        }

        private List<ItemDataSO> GetTargetRewardItems()
        {
            List<ItemDataSO> targetItems = new();
            List<ItemType> allowedTypes = rewardContainer.GetAllowedTypes();
            if (allowedTypes == null || allowedTypes.Count == 0)
            {
                if (itemDB.allItems != null)
                    targetItems.AddRange(itemDB.allItems);
                return targetItems;
            }

            foreach (ItemType type in allowedTypes)
            {
                targetItems.AddRange(itemDB.GetItemsByType(type));
            }

            return targetItems;
        }

        private List<ItemDataSO> GetRandomRewardItems(List<ItemDataSO> targetItems, int count)
        {
            List<ItemDataSO> resultItems = new();
            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, targetItems.Count);
                resultItems.Add(targetItems[index]);
            }

            return resultItems;
        }

        public void Exit()
        {
            if (!_isPlayerInside)
                return;
            TimeController.Instance.TimeScale = _beforeTimeScale;
            if (_currentEnemy != null && !_currentEnemy.IsDead)
            {
                _currentEnemy.OnDeadEvent.RemoveListener(HandleBossDead);
                _currentEnemy.ReleaseToPool();
            }

            _currentEnemy = null;
            _isPlayerInside = false;
            //스테이지 끝남
        }
    }
}
