using Code.InventorySystems.Equipments;
using Code.InventorySystems.Items;
using Code.Players;
using Scripts.Combat.ItemObjects;
using UnityEngine;
using Work.LKW.Code.Items;

namespace InGame.InventorySystem
{
    public class EquipSlot : ItemSlot
    {
        public EquipSlot(ItemBase item, EquipSlotDefine define) : base(item)
        {
            Debug.Assert(item == null || item is EquipableItem, "Invalid Item");

            EquipSlotType = define.allowedEquipSlot;
            EquipPartType = define.equipPart;
            CanHandle = define.canHandle;
            Index = define.index + (int)SlotType.Equip;
        }
        
        public EquipableItem Equipable => Item as EquipableItem;
        public EquipSlotType EquipSlotType { get; private set; }
        public EquipPartType EquipPartType { get; private set; }
        public ItemObject ItemObject => Equipable?.ItemObject;
        public bool CanHandle { get; private set; }

        public bool CanEquip(ItemBase item)
        {
            if (item == null) return true;
            
            if(item is EquipableItem equipableItem)
                return equipableItem.ItemData.itemType.IsAssignableTo(EquipSlotType);
            return false;
        }
    }
}