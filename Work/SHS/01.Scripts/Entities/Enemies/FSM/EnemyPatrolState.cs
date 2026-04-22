using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.Targetings.Events;
using Scripts.Enemies.States;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyPatrolState : EnemyExecuteBehaviourState
    {
        private Vector3 _patrolCenter;
        private float _patrolRadius = 5f;
        private float _waitTime = 2f;
        private float _waitTimer;
        private bool _isWaiting;

        public override float ExecuteTimer => 0.1f;

        public EnemyPatrolState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _patrolCenter = _enemy.SpawnPos;
            _movement.MoveType = NavMoveType.Walk;
            _isWaiting = false;
            _localEventBus.Subscribe<TargetLostEvent>(OnTargetLost);


            SetNewPatrolDestination();
        }

        public override void Update()
        {
            base.Update();

            if (Target != null)
            {
                _enemy.ChangeState(EnemyStateEnum.Chase);
                return;
            }

            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0)
                {
                    _isWaiting = false;
                    SetNewPatrolDestination();
                }
            }
            else if (_movement.IsArrived)
            {
                _isWaiting = true;
                _waitTimer = _waitTime;
                _movement.SetStop(true);
            }

            UpdateMovementAnimation();
        }

        private void SetNewPatrolDestination()
        {
            _movement.SetStop(false);
            Vector2 randomCircle = Random.insideUnitCircle * _patrolRadius;
            Vector3 destination = _patrolCenter + new Vector3(randomCircle.x, 0, randomCircle.y);
            _movement.SetDestination(destination);
        }

        public override void Exit()
        {
            base.Exit();
            _movement.SetStop(false);
            _localEventBus.Unsubscribe<TargetLostEvent>(OnTargetLost);
        }

        public void OnTargetLost(TargetLostEvent evt)
        {
            _enemy.ChangeState(EnemyStateEnum.Chase);
        }
    }
}