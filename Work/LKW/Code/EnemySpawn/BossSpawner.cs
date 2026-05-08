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
using Work.LKW.Code.Items.ItemInfo;

namespace Code.EnemySpawn
{
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] private EnemySO bossSO;
        [SerializeField] private Transform targetTransform,playerTransform;
        [Inject] private PoolManagerMono _poolManager;
        private Enemy _currentEnemy;
        private CharacterMovement _currentPlayer;
        private Vector3 _initPos;
        public void Enter(Entity entity)
        {
            if(_currentEnemy != null)
            {
                _currentEnemy.OnDeadEvent.RemoveListener(Canceled);
                _currentEnemy.ReleaseToPool();
                _currentEnemy = null;
            }
            if (!entity.TryGet<CharacterMovement>(out var movement))
                return;
            _currentEnemy = EnemySpawnUtility.SpawnEnemy(bossSO, targetTransform.position, Quaternion.identity, _poolManager);
            _currentEnemy.OnDeadEvent.AddListener(Canceled);
            _currentPlayer = movement;
            _initPos = movement.transform.position;
            movement.SetPosition(playerTransform.position);
        }

        public void Canceled()
        {
            if (_currentEnemy == null)
                return;
            _currentEnemy.OnDeadEvent.RemoveListener(Canceled);
            if(!_currentEnemy.IsDead)
                _currentEnemy.ReleaseToPool();
            _currentPlayer.SetPosition(_initPos);
            _currentEnemy = null;
            _currentPlayer = null;
            //스테이지 끝남
        }
    }
}
