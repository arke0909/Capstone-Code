using Chipmunk.GameEvents;
using Code.InventorySystems.Items;

namespace Work.Code.GameEvents
{
    public struct ItemEquipRequestEvent : IEvent
    {
        public ItemSlot Slot { get; }

        public ItemEquipRequestEvent(ItemSlot slot)
        {
            Slot = slot;
        }
    }
}