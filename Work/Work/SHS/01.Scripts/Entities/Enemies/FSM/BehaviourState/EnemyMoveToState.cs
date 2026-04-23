using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemyMoveToState : EnemyState
    {
        private bool _lookTarget;

        public EnemyMoveToState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _movement.SetStop(false);
        }

        public override void Update()
        {
            base.Update();

            if (_targetProvider.CurrentTarget == null)
                return;

            if (_movement.IsArrived)
            {
                if (_behaviourManager.CurrentBehaviour != null)
                {
                    _behaviourManager.CurrentBehaviour.SetCooldown();
                }
                _enemy.ChangeState(EnemyStateEnum.Chase);
                return;
            }
            _movement.SetLookAtTarget(_enemy.TargetProvider.CurrentTarget.transform);
            UpdateMovementAnimation();
        }
    }
}
