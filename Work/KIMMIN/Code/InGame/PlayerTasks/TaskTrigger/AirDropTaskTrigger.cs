using Chipmunk.GameEvents;
using Work.Code.GameEvents;

namespace Work.Code.PlayerTasks.TaskTrigger
{
    public class AirDropTaskTrigger : PlayerTaskTrigger
    {
        protected override void OnInitTaskTrigger()
        {
            EventBus.Subscribe<AirdropEvent>(HandleAirDropped);
        }

        private void HandleAirDropped(AirdropEvent evt)
        {
            RaisePlayerTask();
        }

        protected override void OnDisposeTaskTrigger()
        {
            EventBus.Unsubscribe<AirdropEvent>(HandleAirDropped);
        }
    }
}
