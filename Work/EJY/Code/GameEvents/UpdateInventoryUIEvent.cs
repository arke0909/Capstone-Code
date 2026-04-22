using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.InventorySystems.Equipments;
using Code.InventorySystems.Items;
using Code.Players;
using InGame.InventorySystem;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.GameEvents
{
    public struct UpdateInventoryUIMessageEvent : IEvent
    {
    }
    
    public struct UpdateHotbarUIMessageEvent : IEvent
    {
    }
    
    public struct UpdateInventoryUIEvent : IEvent
    {
        public List<ItemSlot> ItemSlots;
        public bool isPlayerInventory;
        public int SlotCnt;
    }
    public struct UpdateEquipUIEvent : IEvent
    {
        public List<EquipSlot> EquipSlots;

        public UpdateEquipUIEvent(List<EquipSlot> equipSlots)
        {
            EquipSlots = equipSlots;
        }
    }
}