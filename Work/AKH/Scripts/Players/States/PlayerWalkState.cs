using Chipmunk.ComponentContainers;
using UnityEngine;

namespace Scripts.Players.States
{
    public class PlayerWalkState : PlayerLocomotionCombatState
    {
        public PlayerWalkState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Walk;
        }
    }
}
