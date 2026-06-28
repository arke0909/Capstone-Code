using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.FSM;
using Scripts.FSM.Events;

namespace SHS.Scripts.Entities.Rigings
{
    public class EnemyItemGrabRiggingController : ItemGrabRiggingController,
        ILocalEventSubscriber<StateChangedEvent<EnemyStateEnum>>
    {
        public void OnLocalEvent(StateChangedEvent<EnemyStateEnum> eventData)
        {
            if (ShouldUseGrabIK(eventData.CurrentState))
                SetMode(GrabRigMode.IkGrab);
            else
                SetMode(GrabRigMode.AnimationHolder);
        }

        private static bool ShouldUseGrabIK(EnemyStateEnum state)
        {
            return state == EnemyStateEnum.Aim ||
                   state == EnemyStateEnum.Attack;
        }
    }
}
