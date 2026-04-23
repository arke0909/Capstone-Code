using System;
using Code.InventorySystems.Equipments;
using Code.Players;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.InventorySystems.Items
{
    public static class EquipExpansion
    {
        public static EquipSlotType GetEquipSlotType(this ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.MeleeWeapon:
                    return EquipSlotType.Melee;
                case ItemType.Gun:
                    return EquipSlotType.Gun;
                case ItemType.Armor:
                    return EquipSlotType.Armor;
                case ItemType.Helmet:
                    return EquipSlotType.Helmet;
                default:
                    return EquipSlotType.None;
            }
        }
        
        public static bool IsAssignableTo(this ItemType itemType, EquipSlotType slotType)
        {
            switch (itemType)
            {
                case ItemType.Gun:
                    return slotType == EquipSlotType.Gun;

                case ItemType.MeleeWeapon:
                    return slotType == EquipSlotType.Melee;

                case ItemType.Armor:
                    return slotType == EquipSlotType.Armor;

                case ItemType.Helmet:
                    return slotType == EquipSlotType.Helmet;
                default:
                    return false;
            }
        }

        public static EquipPartType GetEquipType(this EquipSlotType equipSlotType)
        {
            switch (equipSlotType)
            {
                case EquipSlotType.None:
                    return EquipPartType.None;
                case EquipSlotType.Gun:
                    return EquipPartType.Hand;
                case EquipSlotType.Melee:
                    return EquipPartType.Hand;
                case EquipSlotType.Helmet:
                    return EquipPartType.Helmet;
                case EquipSlotType.Armor:
                    return EquipPartType.Armor;
                default:
                    return EquipPartType.None;
            }
        }
    }
    
    
}