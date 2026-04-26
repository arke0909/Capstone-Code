using System;
using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftModel
    {
        public PlayerInventory Inventory { get; private set; }

        public CraftModel(Player player)
        {
            Inventory = player.Get<PlayerInventory>();
        }
        
        public bool TryCraft(CraftTreeSO tree)
        {
            if (!Inventory.TryConsume(tree.ConsumeItems))
                return false;

            ItemCreateData result = tree.Item.CreateItem();
            Inventory.TryAddItem(result.Item, tree.Count);
            return true;
        }
    }
}