using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Players;
using Scripts.Players.States;
using UnityEngine;
using Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftContext
    {
        public PlayerInventory TargetInventory { get; set; }
        public CraftTreeSO TargetTreeSO { get; set; }
        public Dictionary<ItemDataSO, int> ConsumeItems { get; set; }
        public ItemDataSO[] AutoCraftedItems { get; set; }
        
        public CraftContext(PlayerInventory targetInventory, CraftTreeSO targetTreeSO,
            Dictionary<ItemDataSO, int> consumeItems = null, ItemDataSO[] autoCraftedItems = null)
        {
            TargetInventory = targetInventory;
            TargetTreeSO = targetTreeSO;
            ConsumeItems = consumeItems ?? targetTreeSO?.ConsumeItems;
            AutoCraftedItems = autoCraftedItems;
        }
        
        public void Deconstruct(out PlayerInventory inventory, out CraftTreeSO targetTree)
        {
            inventory = TargetInventory;
            targetTree = TargetTreeSO;
        }
    }

    public enum CraftRequestResult
    {
        Success,
        NotEnoughMaterials,
        InventoryFull
    }
    
    public class CraftModel
    {
        private Player _player;
        public PlayerInventory Inventory { get; }
        
        public CraftModel(Player player)
        {
            Inventory = player.Get<PlayerInventory>();
            _player = player;
        }
        
        public CraftRequestResult TryCraft(CraftTreeSO tree)
        {
            if (!TryGetConsumeItems(tree, out Dictionary<ItemDataSO, int> consumeItems,
                    out ItemDataSO[] autoCraftedItems))
                return CraftRequestResult.NotEnoughMaterials;

            if (!CanAddCraftResult(tree, consumeItems))
                return CraftRequestResult.InventoryFull;

            _player.Blackboard.Set("SelectedCraftSO", new CraftContext(Inventory, tree, consumeItems,
                autoCraftedItems));
            _player.ChangeState(PlayerStateEnum.CraftItem);
            return CraftRequestResult.Success;
        }

        public bool CanCraft(CraftTreeSO tree)
        {
            return TryGetConsumeItems(tree, out _, out _);
        }

        private bool TryGetConsumeItems(CraftTreeSO tree, out Dictionary<ItemDataSO, int> consumeItems,
            out ItemDataSO[] autoCraftedItems)
        {
            consumeItems = new Dictionary<ItemDataSO, int>();
            List<ItemDataSO> autoCraftedItemList = new();
            autoCraftedItems = null;

            if (tree == null || tree.Root?.Item == null)
                return false;

            HashSet<CraftTreeSO> visiting = new();
            int count = tree.isBinary ? 2 : 3;

            for (int i = 1; i <= count && i < tree.nodeList.Count; i++)
            {
                NodeData node = tree.nodeList[i];

                if (node?.Item == null)
                    continue;

                if (!TryResolveNode(node, node.Count, consumeItems, visiting, autoCraftedItemList))
                    return false;
            }

            autoCraftedItems = autoCraftedItemList.ToArray();
            return true;
        }

        private bool TryResolveNode(NodeData node, int needCount,
            Dictionary<ItemDataSO, int> consumeItems, HashSet<CraftTreeSO> visiting,
            List<ItemDataSO> autoCraftedItems)
        {
            consumeItems.TryGetValue(node.Item, out int plannedCount);
            int ownedCount = Mathf.Max(0, Inventory.GetItemCount(node.Item) - plannedCount);
            int consumeCount = Mathf.Min(ownedCount, needCount);

            if (consumeCount > 0)
                AddConsumeItem(consumeItems, node.Item, consumeCount);

            int remainCount = needCount - consumeCount;
            if (remainCount <= 0)
                return true;

            if (node.Tree == null || !visiting.Add(node.Tree))
                return false;

            int craftCount = Mathf.CeilToInt((float)remainCount / node.Tree.Count);
            int childCount = node.Tree.isBinary ? 2 : 3;
            AddAutoCraftedItem(autoCraftedItems, node.Tree.Item);

            for (int i = 1; i <= childCount && i < node.Tree.nodeList.Count; i++)
            {
                NodeData childNode = node.Tree.nodeList[i];

                if (childNode?.Item == null)
                    continue;

                if (!TryResolveNode(childNode, childNode.Count * craftCount, consumeItems, visiting,
                        autoCraftedItems))
                    return false;
            }

            visiting.Remove(node.Tree);
            return true;
        }

        private bool CanAddCraftResult(CraftTreeSO tree, Dictionary<ItemDataSO, int> consumeItems)
        {
            return Inventory.CanAddItemAfterConsume(tree.Item, tree.Count, consumeItems);
        }

        private static void AddConsumeItem(Dictionary<ItemDataSO, int> consumeItems, ItemDataSO item, int count)
        {
            if (consumeItems.TryAdd(item, count))
                return;

            consumeItems[item] += count;
        }

        private static void AddAutoCraftedItem(List<ItemDataSO> autoCraftedItems, ItemDataSO item)
        {
            if (item == null || autoCraftedItems.Contains(item))
                return;

            autoCraftedItems.Add(item);
        }
    }
}
