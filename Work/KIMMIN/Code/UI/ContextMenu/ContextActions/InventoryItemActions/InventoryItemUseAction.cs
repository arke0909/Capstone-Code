using Code.InventorySystems.Items;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Work.Code.UI.ContextMenu.InventoryItemActions
{
    public class InventoryItemUseAction : BaseContextAction<ItemSlot>
    {
        public override bool CheckCondition(ItemSlot data)
        {
            return true;
        }

        public override bool CanShow(ItemSlot data)
        {
            return data.Item is IUsable;
        }

        public override void OnAction(ItemSlot data)
        {
            if(data.Item is IUsable usable)
                usable.Use(_owner);
        }
    }
}