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
    public interface IUpdateInventoryUIEvent : IEvent
    {
        List<ItemSlot> ItemSlots { get; }
        int SlotCnt { get; }
    }
    public struct UpdateLeftInventoryUIEvent : IUpdateInventoryUIEvent
    {
        public List<ItemSlot> ItemSlots { get; set; }

        public int SlotCnt { get; set; }
    }
    public struct UpdateRightInventoryUIEvent : IUpdateInventoryUIEvent
    {
        public List<ItemSlot> ItemSlots { get; set; }
        public int SlotCnt { get; set; }
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