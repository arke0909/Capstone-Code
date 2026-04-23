using Chipmunk.GameEvents;
using Code.UI.Minimap;
using Code.UI.Minimap.Core;

namespace Code.Events
{
    public struct RemoveMinimapElementEvent : IEvent
    {
        public string ID { get; }

        public RemoveMinimapElementEvent(string id)
        {
            ID = id;
        }
    }
}