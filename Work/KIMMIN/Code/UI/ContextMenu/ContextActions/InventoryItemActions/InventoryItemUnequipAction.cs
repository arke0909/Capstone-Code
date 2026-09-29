using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using Work.Code.GameEvents;
using Code.Items;
using InGame.InventorySystem;

namespace Work.Code.UI.ContextMenu.InventoryItemActions
{
    public class InventoryItemUnequipAction : BaseContextAction<ItemSlot>
    {
        public override bool CheckCondition(ItemSlot data)
        {
            return true;
        }

        public override bool CanShow(ItemSlot data)
        {
            return data is EquipSlot &&
                   data.Item is EquipableItem &&
                   data.Item is not IUsable;
        }

        public override void OnAction(ItemSlot data)
        {
            EventBus.Raise(new ItemEquipRequestEvent(data));
        }
    }
}
