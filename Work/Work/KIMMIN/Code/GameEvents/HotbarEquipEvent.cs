using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.Hotbar;
using InGame.InventorySystem;
using Work.LKW.Code.Items;

namespace Code.GameEvents
{
    public struct EquipHotbarEvent : IEvent
    {
        public int Index { get; }
        public ItemBase Item { get; }

        public EquipHotbarEvent(int index, ItemBase item)
        {
            Index = index;
            Item = item;
        }
    }
    
    public struct UnEquipHotbarEvent : IEvent
    {
        public int Index { get; }
        public UnEquipHotbarEvent(int index)
        {
            Index = index;
        }
    }
    
    public struct UpdateHotbarUIEvent : IEvent
    {
        public List<HotbarSlot> EquipSlots;

        public UpdateHotbarUIEvent(List<HotbarSlot> equipSlots)
        {
            EquipSlots = equipSlots;
        }
    }
}