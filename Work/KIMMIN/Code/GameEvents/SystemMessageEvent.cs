using Chipmunk.GameEvents;

namespace Work.Code.GameEvents
{
    public readonly struct SystemMessageEvent : IEvent
    {
        public string Message { get; }

        public SystemMessageEvent(string message)
        {
            Message = message;
        }
    }
}
