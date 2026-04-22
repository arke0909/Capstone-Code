using Chipmunk.GameEvents;

namespace Code.Hotbar
{
    public struct HotbarUseEvent : IEvent
    {
        public int Index { get; }

        public HotbarUseEvent(int index)
        {
            Index = index;
        }
    }
}