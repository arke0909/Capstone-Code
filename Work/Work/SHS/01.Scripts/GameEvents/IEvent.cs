using Chipmunk.GameEvents;

namespace Chipmunk.GameEvents
{
    public interface IEvent
    {
        void Raise()
        {
            EventBus.Raise(this);
        }
    }
}