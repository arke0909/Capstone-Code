using Chipmunk.Library.Utility.GameEvents.Local;
using Code.Items;
using InGame.InventorySystem;

namespace Code.GameEvents
{
    public struct  EquipItemEvent : ILocalEvent
    {
        public EquipSlot EquipSlot { get; private set; }

        public EquipItemEvent(EquipSlot equipSlot)
        {
            EquipSlot = equipSlot;
        }
    }
    
    public struct UnequipItemEvent : ILocalEvent
    {
        public EquipSlot EquipSlot { get; private set; }
        public EquipableItem EquippedItem { get; private set; }

        public UnequipItemEvent(EquipSlot equipSlot, EquipableItem equippedItem)
        {
            EquipSlot = equipSlot;
            EquippedItem = equippedItem;
        }
    }
}