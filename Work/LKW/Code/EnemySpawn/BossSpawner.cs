using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.Items;
using Code.SHS.Entities.Enemies;
using Code.TimeSystem;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Areas;
using Scripts.Entities;
using Scripts.Players;
using Sirenix.OdinInspector;
using UnityEngine;
using Work.Code.GameEvents;
using Code.Items.ItemInfo;
using Code.ItemContainers;
using Scripts.GameSystem.Structures;

namespace Code.EnemySpawn
{
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] private EnemySO bossSO;
        [SerializeField] private Transform targetTransform,playerTransform;
        [SerializeField] private ItemContainer rewardContainer;
        [SerializeField] private List<ItemDataSO> rewardItems = new();
        [SerializeField] private ItemDataBaseSO itemDB;
        [SerializeField] private InvokeCallbackStructure returnStructure;
        [Inject] private PoolManagerMono _poolManager;
        private Enemy _currentEnemy;
        private Vector3 _initPos;
        private Vector3 _returnStructurePos;
        private Vector3 _rewardContainerPos;
        private float _beforeTimeScale;
        private void Start()
        {
            returnStructure.Init(Exit, null);
            _returnStructurePos = returnStructure.transform.position;
            _rewardContainerPos = rewardContainer.transform.position;
            rewardContainer.gameObject.SetActive(false);
        }

        [ContextMenu("Spawn")]
        private void Spawn()
        {
            _currentEnemy = EnemySpawnUtility.SpawnEnemy(bossSO, targetTransform.position, Quaternion.identity, _poolManager);
            _currentEnemy.OnDeadEvent.AddListener(HandleBossDead);
        }
        public void Enter(Entity entity)
        {
            if(_currentEnemy != null)
            {
                _currentEnemy.OnDeadEvent.RemoveListener(HandleBossDead);
                _currentEnemy.ReleaseToPool();
                _currentEnemy = null;
            }
            if (!entity.TryGet<CharacterMovement>(out var movement))
                return;
            _beforeTimeScale = TimeController.Instance.TimeScale;
            TimeController.Instance.TimeScale = 0;
            _currentEnemy = EnemySpawnUtility.SpawnEnemy(bossSO, targetTransform.position, Quaternion.identity, _poolManager);
            _currentEnemy.OnDeadEvent.AddListener(HandleBossDead);
            _initPos = movement.transform.position;
            movement.SetPositionImmediately(playerTransform.position);
            returnStructure.Despawn();
        }

        private void HandleBossDead()
        {
            _currentEnemy?.OnDeadEvent.RemoveListener(HandleBossDead);
            SpawnRewardContainer();
            returnStructure.Spawn(_returnStructurePos);
        }

        private void SpawnRewardContainer()
        {
            if (rewardContainer == null)
                return;

            rewardContainer.transform.position = _rewardContainerPos;
            rewardContainer.gameObject.SetActive(true);
            SetUpRewardContainer();
        }

        private void SetUpRewardContainer()
        {
            if (rewardContainer.Inventory == null)
                return;

            if (rewardItems != null && rewardItems.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, rewardItems.Count);
                rewardContainer.Inventory.SetUpItem(rewardItems[index]);
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

        public void Exit(Entity entity)
        {
            if (_currentEnemy == null)
                return;
            TimeController.Instance.TimeScale = _beforeTimeScale;
            if(!_currentEnemy.IsDead)
                _currentEnemy.ReleaseToPool();
            _currentEnemy = null;
            returnStructure.Despawn();
            rewardContainer?.gameObject.SetActive(false);
            entity.Get<CharacterMovement>().SetPositionImmediately(_initPos);
            //스테이지 끝남
        }
    }
}
