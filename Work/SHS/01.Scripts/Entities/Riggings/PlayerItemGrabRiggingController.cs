using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.FSM.Events;
using Scripts.Players.States;

namespace SHS.Scripts.Entities.Rigings
{
    public class PlayerItemGrabRiggingController : ItemGrabRiggingController,
        ILocalEventSubscriber<StateChangedEvent<PlayerStateEnum>>
    {
        public void OnLocalEvent(StateChangedEvent<PlayerStateEnum> eventData)
        {
            if (ShouldUseGrabIK(eventData.CurrentState))
                SetMode(GrabRigMode.IkGrab);
            else
                SetMode(GrabRigMode.AnimationHolder);
        }

        private static bool ShouldUseGrabIK(PlayerStateEnum state)
        {
            return state == PlayerStateEnum.Aim ||
                   state == PlayerStateEnum.Attack || state == PlayerStateEnum.Idle || state == PlayerStateEnum.Walk;
        }
    }
}
