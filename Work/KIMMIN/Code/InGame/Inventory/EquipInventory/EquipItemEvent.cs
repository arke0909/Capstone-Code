using System;
using Chipmunk.GameEvents;
using Code.InventorySystems.Equipments;
using Code.InventorySystems.Items;
using Work.LKW.Code.Items;

namespace InGame.InventorySystem
{
    public struct EquipByDragEvent : IEvent
    {
        public int Index { get; }
        public ItemSlot StartSlot { get; }
        public EquipByDragEvent(int index, ItemSlot startSlot)
        {
            Index = index;
            StartSlot = startSlot;
        }
    }
    
    public struct UnEquipByDragEvent : IEvent
        {
            public EquipSlot EquipSlot { get; }
            public ItemSlot TargetSlot { get; }

            public UnEquipByDragEvent(ItemBase item, EquipSlot equipSlot, ItemSlot targetSlot)
            {
                EquipSlot = equipSlot;
                this.TargetSlot = targetSlot;
            }
        }
}