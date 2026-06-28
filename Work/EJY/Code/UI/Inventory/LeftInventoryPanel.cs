using Code.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using Code.Items;

namespace Code.UI.Inventory
{
    public class LeftInventoryPanel : InventoryPanel<UpdateLeftInventoryUIEvent>
    {
        protected override void HandleClick(ItemSlot slot)
        {
            base.HandleClick(slot);
            if (slot.Item is EquipableItem item)
            {
            }
        }
    }
}
