using Chipmunk.GameEvents;
using Code.InventorySystems;
using Code.ItemContainers;

namespace Work.LKW.Code.Events
{
    public struct OpenRightInventoryEvent : IEvent
    {
        public Inventory RightInventory{ get; }

        public OpenRightInventoryEvent(Inventory Inventory)
        {
            this.RightInventory = Inventory;
        }
    }
}