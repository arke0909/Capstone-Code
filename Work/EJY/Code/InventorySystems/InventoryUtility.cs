using System;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using UnityEngine;

namespace Code.InventorySystems
{
    public static class InventoryUtility
    {
        private static readonly SlotType[] OrderedTypes =
        {
            SlotType.Inventory,
            SlotType.Equip,
            SlotType.Hotbar
        };

        public static bool TryGetSlot<T>(ItemSlot itemSlot, SlotType slotType, out T slot) where T : ItemSlot
        {
            slot = null;

            if (itemSlot == null)
                return false;

            if (!CheckSlotType(itemSlot.Index, slotType))
                return false;

            slot = itemSlot as T;
            return slot != null;
        }
        
        public static bool CheckSlotType(int index, SlotType slotType)
        {
            int current = (int)slotType;
            int next = GetNextBoundary(slotType);

            return index >= current && index < next;
        }

        public static SlotType GetSlotType(int index)
        {
            for (int i = OrderedTypes.Length - 1; i >= 0; i--)
            {
                if (index >= (int)OrderedTypes[i])
                    return OrderedTypes[i];
            }

            return SlotType.None;
        }

        public static int GetLocalIndex(int index)
        {
            if (index == -1) return -1;
            
            SlotType slotType = GetSlotType(index);
            return index - (int)slotType;
        }

        private static int GetNextBoundary(SlotType slotType)
        {
            for (int i = 0; i < OrderedTypes.Length; i++)
            {
                if (OrderedTypes[i] != slotType)
                    continue;

                if (i == OrderedTypes.Length - 1)
                    return int.MaxValue;

                return (int)OrderedTypes[i + 1];
            }

            return -1;
        }
    }
}
