using System;
using System.Collections.Generic;
using Code.Items.ItemInfo;
using DewmoLib.Dependencies;
using UnityEngine;

namespace Work.Code.Craft
{
    [Provide]
    public class CraftPinItemContainer : MonoBehaviour, IDependencyProvider
    {
        private readonly HashSet<CraftTreeSO> _pinnedTrees = new();
        private readonly List<CraftTreeSO> _pinnedTreeList = new();
        private readonly HashSet<ItemDataSO> _items = new();

        public IReadOnlyCollection<ItemDataSO> Items => _items;
        public event Action OnChanged;

        public void AddTree(CraftTreeSO tree)
        {
            if (tree == null || !_pinnedTrees.Add(tree))
                return;

            _pinnedTreeList.Add(tree);
            RefreshItems();
        }

        public void RemoveTree(CraftTreeSO tree)
        {
            if (tree == null || !_pinnedTrees.Remove(tree))
                return;

            _pinnedTreeList.Remove(tree);
            RefreshItems();
        }

        public bool TryGetFirstTree(out CraftTreeSO tree)
        {
            tree = _pinnedTreeList.Count > 0 ? _pinnedTreeList[0] : null;
            return tree != null;
        }

        public bool TryGetTree(ItemType itemType, Rarity rarity, out CraftTreeSO tree)
        {
            foreach (CraftTreeSO pinnedTree in _pinnedTreeList)
            {
                if (pinnedTree.Item.itemType != itemType || pinnedTree.Item.rarity != rarity)
                    continue;

                tree = pinnedTree;
                return true;
            }

            tree = null;
            return false;
        }

        public bool Contains(ItemDataSO item)
        {
            return item != null && _items.Contains(item);
        }

        private void RefreshItems()
        {
            _items.Clear();

            foreach (CraftTreeSO tree in _pinnedTrees)
            {
                foreach (ItemDataSO item in tree.NeedItemType)
                {
                    if (item != null)
                        _items.Add(item);
                }
            }

            OnChanged?.Invoke();
        }
    }
}
