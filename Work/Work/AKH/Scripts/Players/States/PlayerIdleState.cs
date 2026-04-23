using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Scripts.Players.States
{
    public class PlayerIdleState : PlayerLocomotionCombatState
    {
        public PlayerIdleState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
        public override void Enter()
        {
            base.Enter();
            _movement.StopImmediately();
            _movement.MoveType = MoveType.Idle;
        }
        public override void Update()
        {
            base.Update();
        }
    }
}
