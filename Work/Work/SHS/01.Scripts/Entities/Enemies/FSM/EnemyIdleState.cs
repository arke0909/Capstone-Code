using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events;
using Code.SHS.Entities.Enemies.Targetings.Events;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyIdleState : EnemyState
    {
        private float _idleTime;
        private float _timeToPatrol = 3f;

        public EnemyIdleState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _movement.MoveType = NavMoveType.Idle;
            _movement.SetStop(true);
            _idleTime = 0f;
            _localEventBus.Subscribe<TargetLostEvent>(OnTargetLost);
        }

        public override void Update()
        {
            base.Update();

            if (RemainTarget != null)
            {
                _enemy.ChangeState(EnemyStateEnum.Chase);
                return;
            }

            _idleTime += Time.deltaTime;
            if (_idleTime >= _timeToPatrol)
            {
                _enemy.ChangeState(EnemyStateEnum.Patrol);
            }
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