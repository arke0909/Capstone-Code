using System;
using Code.InventorySystems;
using DewmoLib.Utiles;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.InventorySystems.Items
{
    public enum SlotType
    {
        None = -1,
        Inventory = 0,
        Equip = 5000,
        Hotbar = 10000,
        ItemContainer = 15000,
    }
    
    [Serializable]
    public class ItemSlot
    {
        [field: SerializeField] public Inventory OwnerInventory { get; protected set; }
        [field: SerializeReference] public ItemBase Item { get; protected set; }
        [field: SerializeField] public int Stack { get; protected set; }
        public bool IsFull => !IsBlank && Stack == Item.ItemData.maxStack;
        public bool IsBlank => Item == null;
        public int Index { get; set; }

        public ItemSlot(ItemBase item, int stack = 0)
        {
            SetData(item, stack);
        }

        public void SetIndex(int idx) => Index = idx;
        
        public void SetOwner(Inventory ownerInventory) => OwnerInventory = ownerInventory;
        
        public void SetData(ItemBase item, int stack = 0)
        {
            Item = item;
            bool slotEmpty = item == null;
            if (slotEmpty)
            {
                Stack = 0;
                return;
            }
            
            Stack = Mathf.Clamp(stack, 1, item.ItemData.maxStack);
            Item.SetOwner(OwnerInventory.Owner);
        }
        
        public int AddItem(int amount = 1)
        {
            if (Item == null || amount <= 0) return amount;

            int total = Stack + amount;
            int remain = 0;

            if (total > Item.ItemData.maxStack)
            {
                remain = total - Item.ItemData.maxStack;
                Stack = Item.ItemData.maxStack;
            }
            else
            {
                Stack += amount;
            }
            
            return remain;
        }

        public int RemoveItem(int amount = 1)
        {
            if (Item == null || amount <= 0)
                return amount;

            int removed = Mathf.Min(amount, Stack);
            Stack -= removed;

            if (Stack <= 0)
                Clear();

            return amount - removed;
        }


        public void Clear()
        {
            Item = null;
            Stack = 0;
        }
    }
}