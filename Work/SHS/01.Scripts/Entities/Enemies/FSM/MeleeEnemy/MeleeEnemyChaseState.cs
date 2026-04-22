using Chipmunk.ComponentContainers;
using Scripts.Enemies.EnemyBehaviours;
using Scripts.Enemies.States;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class MeleeEnemyChaseState : EnemyExecuteBehaviourState
    {
        public MeleeEnemyChaseState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override float ExecuteTimer => 0f;

        public override void Enter()
        {
            base.Enter();
            _movement.MoveType = NavMoveType.Sprint;
            _movement.SetLookAtTarget(null);
            _movement.SetStop(false);
        }

        public override void Update()
        {
            base.Update();

            if (RemainTarget == null && _movement.IsArrived)
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
                return;
            }
            Vector3 destination = Target != null ? Target.transform.position : _targetProvider.LastTargetPosition;
            _movement.SetDestination(destination);
            UpdateMovementAnimation();
        }

        public override void Exit()
        {
            base.Exit();
            if (RemainTarget == null)
                _targetProvider.TargetLost(_targetProvider.LastTargetPosition);
        }
    }
}