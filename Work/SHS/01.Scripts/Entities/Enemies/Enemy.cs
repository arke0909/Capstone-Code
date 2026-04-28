using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.Combat;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies.Combat;
using Code.SHS.Entities.Enemies.Events.Local;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Entities.Enemies.Groups;
using Code.SHS.Entities.Enemies.Skills;
using Code.SHS.Targetings.Enemies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Combat.Fovs;
using Scripts.Entities;
using Scripts.FSM;
using Scripts.Players;
using SHS.Scripts.Combats.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Code.SHS.Entities.Enemies
{
    public class Enemy : Entity, IKnockbackable, IStateEntity, IFindable, IPullable, IPoolable
    {
        [SerializeField] public LayerMask playerLayerMask;
        [SerializeField] public EnemySO test;
        public float movingRange = 5;

        public PoolItemSO PoolItem => _runtimePoolItem;
        public GameObject GameObject => gameObject;
        public TargetProvider TargetProvider { get; private set; }
        public EnemyStateMachineBehavior StateMachineBehavior { get; private set; }
        public EnemySO EnemyData { get; private set; }
        public NavMovement NavMovement { get; private set; }
        public GroupProvider GroupProvider { get; private set; }
        public int SightCount { get; set; }
        [field: SerializeField] public UnityEvent<bool> OnFound { get; private set; }
        [field: SerializeField] public Vector3 SpawnPos { get; private set; }

        private LocalEventBus _localEventBus;
        private Pool _pool;
        private PoolItemSO _runtimePoolItem;
        private int _defaultLayer;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            TargetProvider = ComponentContainer.GetComponent<TargetProvider>();
            NavMovement = ComponentContainer.GetComponent<NavMovement>(true);
            StateMachineBehavior = ComponentContainer.GetComponent<EnemyStateMachineBehavior>(true);
            GroupProvider = ComponentContainer.GetComponent<GroupProvider>();
            _localEventBus = ComponentContainer.GetComponent<LocalEventBus>();
            _defaultLayer = gameObject.layer;
            OnDeadEvent.AddListener(HandleEnemyDead);
        }

        private void Start()
        {
            OnFound?.Invoke(((IFindable)this).IsFounded);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Semicolon))
            {
                SpawnEnemy(transform.position, test);
            }
        }

        private void HandleEnemyDead()
        {
            IsDead = true;
            gameObject.layer = LayerMask.NameToLayer("AvoidEntity");
            ChangeState(EnemyStateEnum.Dead);
        }

        public void SpawnEnemy(Vector3 position, EnemySO enemyData)
        {
            EnemyData = enemyData;
            SpawnPos = position;
            _runtimePoolItem = enemyData != null ? enemyData.enemyPoolItem : null;
            transform.position = position;

            ResetItem();
            Quaternion spawnRotation = transform.rotation;
            _localEventBus.Raise(new EnemySpawnEvent(enemyData, position, spawnRotation));
        }

        public void ChangeState(EnemyStateEnum newState, bool forced = false)
            => StateMachineBehavior.ChangeState(newState, forced);

        public void ChangeState(StateDataSO stateData)
        {
            if (Enum.TryParse<EnemyStateEnum>(stateData.enumName, out var newState))
            {
                ChangeState(newState);
            }
            else
            {
                Debug.LogWarning($"Can't find state: {stateData.enumName}");
            }
        }

        public override void Stun(float duration)
        {
            EnemyStunState stunState = StateMachineBehavior.StateMachine?.GetState<EnemyStunState>(EnemyStateEnum.Stun);
            if (stunState == null)
                return;

            stunState.SetStunDuration(duration);
            ChangeState(EnemyStateEnum.Stun);
            _localEventBus.Raise(new StunnedEvent(duration));
        }

        public void KnockBack(Vector3 direction, MovementDataSO movementData)
            => NavMovement.KnockBack(direction, movementData);

        public void Founded()
            => OnFound?.Invoke(true);

        public void Escape()
            => OnFound?.Invoke(false);

        public void Pull(Vector3 pullOffset)
        {
            NavMovement.Move(pullOffset);
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
            IsDead = false;
            gameObject.layer = _defaultLayer;
        }

        public void SetRuntimePoolItem(PoolItemSO poolItem)
        {
            _runtimePoolItem = poolItem;
        }

        public void ReleaseToPool()
        {
            if (_pool != null && _runtimePoolItem != null)
            {
                _pool.Push(this);
                return;
            }

            Destroy(gameObject);
        }
    }
}
