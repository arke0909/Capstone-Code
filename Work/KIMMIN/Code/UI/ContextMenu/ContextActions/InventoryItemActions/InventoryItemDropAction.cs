using Chipmunk.ComponentContainers;
using Code.InventorySystems.Items;
using Code.Items;
using Code.Players;
using UnityEngine;

namespace Work.Code.UI.ContextMenu.InventoryItemActions
{
    public class InventoryItemDropAction : BaseContextAction<ItemSlot>
    {
        private PlayerInventory _playerInventory;
        private PlayerEquipment _playerEquipment;
        
        public override bool CheckCondition(ItemSlot data)
        {
            return data.Item != null;
        }

        public override bool CanShow(ItemSlot data)
        {
            return CheckCondition(data);
        }

        public override void Init(ItemSlot data)
        {
            base.Init(data);

            _playerInventory = _owner.Get<PlayerInventory>();
            _playerEquipment = _owner.Get<PlayerEquipment>();
        }

        public override void OnAction(ItemSlot data)
        {
            if (data.Item is EquipableItem equipable && equipable.IsEquipped)
            {
                if (_playerEquipment == null || !_playerEquipment.DropEquippedItem(equipable))
                {
                    Debug.LogWarning("Failed to unequip equipped item before dropping.", _playerInventory);
                }

                return;
            }

            _playerInventory.DropItem(data.Item, data.Stack, data);
        }
    }
}
