using Code.InventorySystems.Items;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Work.Code.UI.ContextMenu.InventoryItemActions
{
    public class InventorySkillUpgradeAction : BaseContextAction<ItemSlot>
    {
        public override bool CheckCondition(ItemSlot data)
        {
            return true;
        }

        public override bool CanShow(ItemSlot data)
        {
            return data.Item is EquipableItem equipable && equipable.Skill != null 
                                                   && data.Item is not IUsable;
        }

        public override void OnAction(ItemSlot data)
        {
        }
    }
}