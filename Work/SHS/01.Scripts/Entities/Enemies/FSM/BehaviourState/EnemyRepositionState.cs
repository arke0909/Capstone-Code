using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemyRepositionState : EnemyState
    {
        public EnemyRepositionState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (RemainTarget == null)
                return;

            _movement.SetStop(false);

            Vector3 targetPosition = _enemy.TargetProvider.LastTargetPosition;
            Vector3 nearRandomPosition = targetPosition + Random.insideUnitSphere * _enemy.movingRange;
            nearRandomPosition.y = _enemy.transform.position.y;
            if (!_movement.SetDestinationForce(nearRandomPosition))
            {
                _enemy.ChangeState(EnemyStateEnum.Aim);
                return;
            }
            _movement.MoveType = NavMoveType.Walk;
        }

        public override void Update()
        {
            base.Update();

            if (Target == null)
                return;

            if (_movement.IsArrived)
            {
                _behaviourManager.CurrentBehaviour?.SetCooldown();
                _enemy.ChangeState(EnemyStateEnum.Aim);
                return;
            }

            UpdateMovementAnimation();
        }
    }
}
