using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Scripts.Combat.Datas;

namespace Scripts.Players.States
{
    public struct NoAmmoSoundEvent : IEvent 
    { }
    public class PlayerAimState : PlayerStationaryTurnLocomotionCombatState
    {
        public PlayerAimState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Aim;
        }
    }
}
