using Chipmunk.ComponentContainers;
using Scripts.Enemies.EnemyBehaviours;
using Scripts.Enemies.States;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class MeleeEnemyChaseState : EnemyExecuteBehaviourState
    {
        private static readonly int _walkHash = Animator.StringToHash("Walk");
        private static readonly int _sprintHash = Animator.StringToHash("Sprint");

        private bool _isFinalChase;
        public MeleeEnemyChaseState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override float ExecuteTimer => 0f;

        public override void Enter()
        {
            base.Enter();
            NavMoveType moveType = _sprintStamina != null ? _sprintStamina.GetSprintMoveType() : NavMoveType.Sprint;
            _movement.MoveType = moveType;
            _animator.SetParam(_walkHash, moveType == NavMoveType.Walk);
            _animator.SetParam(_sprintHash, moveType == NavMoveType.Sprint);
            _movement.SetLookAtTarget(null);
            _movement.SetStop(false);
        }

        public override void Update()
        {
            if (RemainTarget == null && _movement.IsArrived)
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
                return;
            }

            NavMoveType moveType = _sprintStamina != null ? _sprintStamina.GetSprintMoveType() : NavMoveType.Sprint;
            _movement.MoveType = moveType;
            _animator.SetParam(_walkHash, moveType == NavMoveType.Walk);
            _animator.SetParam(_sprintHash, moveType == NavMoveType.Sprint);
            Vector3 destination = RemainTarget != null ? RemainTarget.transform.position : _targetProvider.LastTargetPosition;
            _movement.SetDestination(destination);
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
