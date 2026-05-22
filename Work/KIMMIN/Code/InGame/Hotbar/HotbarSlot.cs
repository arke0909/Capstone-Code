using System;
using Code.InventorySystem;
using Code.InventorySystems.Items;
using UnityEngine;
using Code.Items;

namespace Code.Hotbar
{
    public class HotbarSlot : ItemSlot
    {
        public Action<int> OnEquip;
        
        public HotbarSlot(ItemBase item) : base(item)
        {
            Debug.Assert(item == null || item is EquipableItem, "Invalid Item");
        }
        
        public HotbarType HotbarType { get; set; }
    }
}