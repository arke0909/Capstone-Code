using Code.InventorySystems.Items;
using Work.LKW.Code.Items;

namespace Work.Code.UI.ContextMenu.InventoryItemActions
{
    public class InventoryItemDropAction : BaseContextAction<ItemSlot>
    {
        public override bool CheckCondition(ItemSlot data)
        {
            return true;
        }

        public override void OnAction(ItemSlot data)
        {
            
        }
    }
}