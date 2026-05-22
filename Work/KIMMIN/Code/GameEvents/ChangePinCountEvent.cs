using Chipmunk.GameEvents;

namespace Work.Code.GameEvents
{
    public struct ChangePinCountEvent : IEvent
    {
        public int Count { get; }

        public ChangePinCountEvent(int count)
        {
            Count = count;
        }
    }
}