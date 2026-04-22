using System;
using Chipmunk.GameEvents;
using Code.InventorySystems.Equipments;
using Code.InventorySystems.Items;
using Work.LKW.Code.Items;

namespace InGame.InventorySystem
{
    public struct EquipByDragEvent : IEvent
    {
        public ItemBase Item { get; }
        public int Index { get; }
        public ItemSlot StartSlot { get; }
        public Action OnSuccessCallback { get; } 
        public EquipByDragEvent(ItemBase item, int index, ItemSlot startSlot, Action onSuccessCallback)
        {
            this.Item = item;
            Index = index;
            StartSlot = startSlot;
            OnSuccessCallback = onSuccessCallback;
        }
    }
    
    public struct UnEquipByDragEvent : IEvent
        {
            public ItemBase Item { get; }
            public EquipSlot EquipSlot { get; }
            public ItemSlot TargetSlot { get; }
    
            public UnEquipByDragEvent(ItemBase item, EquipSlot equipSlot, ItemSlot targetSlot)
            {
                this.Item = item;
                EquipSlot = equipSlot;
                this.TargetSlot = targetSlot;
            }
        }
}