using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using Work.LKW.Code.Items;

namespace Code.GameEvents
{
    public struct HoveringSlotEvent : IEvent
    {
        public ItemSlotUI ItemSlot;
    }
}