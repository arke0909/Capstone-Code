using Code.InventorySystems.Items;
using UnityEngine;
using Code.Items;
using Scripts.Players.States;

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
            return data.Item is UsableItem usable;
        }

        public override void OnAction(ItemSlot data)
        {
            if (data.Item is UsableItem usable)
            {
                ItemUseContext context = _owner.Blackboard.GetOrDefault<ItemUseContext>("ItemUseContext");
                if (context == null)
                {
                    context = new ItemUseContext();
                    _owner.Blackboard.Set("ItemUseContext", context);
                }
                context.TargetItem = usable;
                
                _owner.ChangeState(PlayerStateEnum.ItemUse);
            }
        }
    }
}