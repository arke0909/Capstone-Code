using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies.Combat;
using Code.SHS.Entities.Enemies.Events.Local;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Targetings.Enemies;
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
using Code.Combat;
using Code.SHS.Entities.Enemies.Groups;

namespace Code.SHS.Entities.Enemies
{
    public class Enemy : Entity, IKnockbackable, IStateEntity, IFindable, IPullable
    {
        [SerializeField] public LayerMask playerLayerMask;

        public float movingRange = 5;

        public TargetProvider TargetProvider { get; private set; }
        public EnemyStateMachineBehavior StateMachineBehavior { get; private set; }
        public EnemySO EnemyData { get; private set; }
        public NavMovement NavMovement { get; private set; }
        public GroupProvider GroupProvider { get; private set; }
        public int SightCount { get; set; }
        [field: SerializeField] public UnityEvent<bool> OnFound { get; private set; }
        [field: SerializeField] public Vector3 SpawnPos { get; private set; }

        private EnemyStunState _stunState;
        private LocalEventBus _localEventBus;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            TargetProvider = ComponentContainer.GetComponent<TargetProvider>();
            NavMovement = ComponentContainer.GetComponent<NavMovement>(true);
            StateMachineBehavior = ComponentContainer.GetComponent<EnemyStateMachineBehavior>(true);
            GroupProvider = ComponentContainer.GetComponent<GroupProvider>();
            _localEventBus = ComponentContainer.GetComponent<LocalEventBus>();
            OnDeadEvent.AddListener(HandleEnemyDead);
        }
        private void Start()
        {
            OnFound?.Invoke(((IFindable)this).IsFounded);
        }

        private void HandleEnemyDead()
        {
            IsDead = true;
            gameObject.layer = LayerMask.NameToLayer("AvoidEntity");
            ChangeState(EnemyStateEnum.Dead);
        }

        public void SpawnEnemy(Vector3 position, EnemySO enemyData)
        {
            LocalEventBus localEventBus = ComponentContainer.GetComponent<LocalEventBus>(true);
            localEventBus.Raise(new EnemySpawnEvent(enemyData));
            EnemyData = enemyData;
            SpawnPos = position;
            _stunState = StateMachineBehavior.StateMachine.GetState<EnemyStunState>(EnemyStateEnum.Stun);
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
            _stunState.SetStunDuration(duration);
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
    }
}