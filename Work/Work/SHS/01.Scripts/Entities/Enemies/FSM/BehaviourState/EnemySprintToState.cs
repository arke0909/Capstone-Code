using Chipmunk.ComponentContainers;
using Code.SHS.Targetings.Enemies;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemySprintToState : EnemyState
    {
        public EnemySprintToState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _movement.SetStop(false);
            _movement.MoveType = NavMoveType.Sprint;
            _movement.SetLookAtTarget(null);
        }

        public override void Update()
        {
            base.Update();

            if (_enemy.TargetProvider.CurrentTarget == null)
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

            UpdateMovementAnimation();
        }
    }
}