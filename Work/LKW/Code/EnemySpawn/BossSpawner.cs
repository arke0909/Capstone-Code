using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
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
        [SerializeField] private InvokeCallbackStructure returnStructure;
        [Inject] private PoolManagerMono _poolManager;
        private Enemy _currentEnemy;
        private Vector3 _initPos;
        private Vector3 _returnStructurePos;
        private float _beforeTimeScale;
        private void Start()
        {
            returnStructure.Init(Exit, null);
            _returnStructurePos = returnStructure.transform.position;
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
            movement.SetPosition(playerTransform.position);
            returnStructure.Despawn();
        }

        private void HandleBossDead()
        {
            _currentEnemy.OnDeadEvent.RemoveListener(HandleBossDead);
            returnStructure.Spawn(_returnStructurePos);
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
            entity.Get<CharacterMovement>().SetPosition(_initPos);
            //스테이지 끝남
        }
    }
}
