using Chipmunk.ComponentContainers;

namespace Scripts.Players.States
{
    public class PlayerIdleState : PlayerStationaryTurnLocomotionCombatState
    {
        public PlayerIdleState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Idle;
        }
        public override void Enter()
        {
            base.Enter();
            _movement.StopImmediately();
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
