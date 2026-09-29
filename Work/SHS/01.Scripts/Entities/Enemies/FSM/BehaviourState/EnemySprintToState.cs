using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.Targetings.Events;
using Code.SHS.Targetings.Enemies;
using Scripts.FSM;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemySprintToState : EnemyState
    {
        private static readonly int _walkHash = Animator.StringToHash("Walk");
        private static readonly int _sprintHash = Animator.StringToHash("Sprint");

        public EnemySprintToState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _movement.SetStop(false);
            NavMoveType moveType = _sprintStamina != null ? _sprintStamina.GetSprintMoveType() : NavMoveType.Sprint;
            _movement.MoveType = moveType;
            _animator.SetParam(_walkHash, moveType == NavMoveType.Walk);
            _animator.SetParam(_sprintHash, moveType == NavMoveType.Sprint);
            _movement.SetLookAtTarget(null);
        }
        protected override void HandleTargetLost(TargetLostEvent @event)
        {

        }
        public override void Update()
        {
            base.Update();

            //if (_enemy.TargetProvider.CurrentTarget == null)
            //    return;

            if (_movement.IsArrived)
            {
                if (_behaviourManager.CurrentBehaviour != null)
                {
                    _behaviourManager.CurrentBehaviour.SetCooldown();
                }

                _enemy.ChangeState(EnemyStateEnum.Chase);
                return;
            }

            NavMoveType moveType = _sprintStamina != null ? _sprintStamina.GetSprintMoveType() : NavMoveType.Sprint;
            _movement.MoveType = moveType;
            _animator.SetParam(_walkHash, moveType == NavMoveType.Walk);
            _animator.SetParam(_sprintHash, moveType == NavMoveType.Sprint);
            UpdateMovementAnimation();
        }

        public override void Exit()
        {
            base.Exit();
            _animator.SetParam(_walkHash, false);
            _animator.SetParam(_sprintHash, false);
        }
    }
}
