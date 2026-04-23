using Chipmunk.GameEvents;

namespace Code.GameEvents
{
    public struct OpenPlayerUIEvent : IEvent
    {
        public bool WithLootInventory { get; }

        public OpenPlayerUIEvent(bool withLootInventory)
        {
            WithLootInventory = withLootInventory;
        }
    }
}