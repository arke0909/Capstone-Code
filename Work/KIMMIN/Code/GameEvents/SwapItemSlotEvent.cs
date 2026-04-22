using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;

namespace Code.GameEvents
{
    public class SwapItemSlotEvent : IEvent
    {
        public ItemSlot StartSlot;
        public ItemSlot TargetSlot;

        public SwapItemSlotEvent(ItemSlot startSlot,ItemSlot targetSlot)
        {
            StartSlot = startSlot;
            TargetSlot = targetSlot;
        }
    }
}