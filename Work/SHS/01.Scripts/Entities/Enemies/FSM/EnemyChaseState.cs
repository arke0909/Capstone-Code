using Chipmunk.ComponentContainers;
using Scripts.Enemies.States;
using Scripts.Entities;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyChaseState : EnemyExecuteBehaviourState
    {
        private static readonly int _walkHash = Animator.StringToHash("Walk");
        private static readonly int _sprintHash = Animator.StringToHash("Sprint");

        public override float ExecuteTimer => 0.1f;

        public EnemyChaseState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            NavMoveType moveType = _sprintStamina != null ? _sprintStamina.GetSprintMoveType() : NavMoveType.Sprint;
            _movement.MoveType = moveType;
            _animator.SetParam(_walkHash, moveType == NavMoveType.Walk);
            _animator.SetParam(_sprintHash, moveType == NavMoveType.Sprint);
            _movement.SetLookAtTarget(null);
            _movement.SetStop(false);
            Vector3 destination = Target != null ? Target.transform.position : _targetProvider.LastTargetPosition;
            _movement.SetDestination(destination);
        }

        public override void Update()
        {
            if (RemainTarget == null && _targetProvider.CanMissTarget &&_movement.IsArrived)
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
                return;
            }

            if (RemainTarget != null)
            {
                float distance = Vector3.Distance(_enemy.transform.position,
                    _targetProvider.CurrentTarget.transform.position);
                if (distance <= _attackRange)
                {
                    _enemy.ChangeState(EnemyStateEnum.Aim);
                    return;
                }
            }

            NavMoveType moveType = _sprintStamina != null ? _sprintStamina.GetSprintMoveType() : NavMoveType.Sprint;
            _movement.MoveType = moveType;
            _animator.SetParam(_walkHash, moveType == NavMoveType.Walk);
            _animator.SetParam(_sprintHash, moveType == NavMoveType.Sprint);
            UpdateMovementAnimation();
            base.Update();
        }

        public override void Exit()
        {
            base.Exit();
            _animator.SetParam(_walkHash, false);
            _animator.SetParam(_sprintHash, false);
            if (RemainTarget == null)
                _targetProvider.TargetLost(_targetProvider.LastTargetPosition);
        }
    }
}
