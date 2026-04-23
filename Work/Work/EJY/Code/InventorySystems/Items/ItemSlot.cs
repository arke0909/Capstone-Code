using System;
using DewmoLib.Utiles;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.InventorySystems.Items
{
    [Serializable]
    public class ItemSlot
    {
        [field: SerializeField] public Inventory OwnerInventory { get; protected set; }
        [field: SerializeReference] public ItemBase Item { get; protected set; }
        [field: SerializeField] public int Stack { get; protected set; }
        public bool IsFull => !IsBlank && Stack == Item.ItemData.maxStack;
        public bool IsBlank => Item == null;

        public ItemSlot(ItemBase item, int stack = 0)
        {
            SetData(item, stack);
        }

        public void SetOwner(Inventory ownerInventory) => OwnerInventory = ownerInventory;
        
        public void SetData(ItemBase item, int stack = 0)
        {
            Item = item;
            bool slotEmpty = item == null;
            Stack = slotEmpty ? 0 : Mathf.Clamp(stack, 1, item.ItemData.maxStack);
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