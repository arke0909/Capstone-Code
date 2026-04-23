using Chipmunk.GameEvents;

namespace Code.GameEvents
{
    public struct PlayerUIEvent : IEvent
    {
        public bool IsEnabled { get; }

        public PlayerUIEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}