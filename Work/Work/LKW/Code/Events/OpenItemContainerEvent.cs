using Chipmunk.GameEvents;
using Work.LKW.Code.ItemContainers;

namespace Work.LKW.Code.Events
{
    public struct OpenItemContainerEvent : IEvent
    {
        public ItemContainer ItemContainer { get; }

        public OpenItemContainerEvent(ItemContainer itemContainer)
        {
            this.ItemContainer = itemContainer;
        }
    }
}