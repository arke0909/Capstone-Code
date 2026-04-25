using Code.InGame.Hotbar;
using Code.InventorySystem;
using Code.InventorySystems.Items;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.Hotbar
{
    public class HotbarSlot : ItemSlot
    {
        public HotbarSlot(ItemBase item) : base(item)
        {
            Debug.Assert(item == null || item is EquipableItem, "Invalid Item");
        }
        
        public HotbarType HotbarType { get; set; }
    }
}