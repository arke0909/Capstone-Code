using Chipmunk.GameEvents;
using Code.Items.ItemInfo;

namespace Work.Code.GameEvents
{
    public readonly struct ItemPickedUpEvent : IEvent
    {
        public ItemDataSO ItemData { get; }

        public ItemPickedUpEvent(ItemDataSO itemData)
        {
            ItemData = itemData;
        }
    }
}
