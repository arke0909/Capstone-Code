using Chipmunk.GameEvents;
using Work.Code.MapEvents;

namespace Work.Code.GameEvents
{
    public struct MapEventStartEvent : IEvent
    {
        public MapEvent MapEvent { get; }
        public float Duration { get; }

        public MapEventStartEvent(MapEvent mapEvent, float duration)
        {
            MapEvent = mapEvent;
            Duration = duration;
        }
    }
}