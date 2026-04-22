using Chipmunk.GameEvents;
using InGame.InventorySystem;
using UnityEngine;

namespace Code.GameEvents
{
    public struct StartDragEvent : IEvent
    {
        public ItemSlotUI ItemSlotUI { get; }

        public StartDragEvent(ItemSlotUI itemSlotUI)
        {
            ItemSlotUI = itemSlotUI;
        }
    }
    
    public struct EndDragEvent : IEvent { }
}