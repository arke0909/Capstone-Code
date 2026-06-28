using Chipmunk.Library.Utility.GameEvents.Local;

namespace Scripts.FSM.Events
{
    public struct StateChangedEvent<TStateEnum> : ILocalEvent where TStateEnum : System.Enum 
    {
        public TStateEnum PreviousState { get; }
        public TStateEnum CurrentState { get; }

        public StateChangedEvent(TStateEnum previousState, TStateEnum currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }
}