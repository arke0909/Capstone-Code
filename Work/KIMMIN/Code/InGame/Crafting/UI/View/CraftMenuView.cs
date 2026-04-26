using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft.View
{
    public class CraftMenuView : MonoBehaviour
    { 
        [FormerlySerializedAs("craftingUIPrefab")] [SerializeField] private CraftItemUI craftUIPrefab;
        [SerializeField] private Transform root; 
        [SerializeField] private Button createButton;

        private readonly Dictionary<ItemDataSO, CraftItemUI> _itemDict = new();
        
        public event Action<CraftTreeSO> OnTreeSelected;
        public event Action<CraftTreeSO> OnRequestCraft;
        public event Action<CraftItemUI, bool> OnPinItem;

        private CraftTreeSO _currentTree;

        public void InitMenuView(CraftTreeListSO craftTreeList)
        {
            foreach (CraftTreeSO tree in craftTreeList.list)
            {
                if (tree.Item != null)
                {
                    InitCraftingItemUI(tree);
                }
            }
            
            createButton.onClick.AddListener(() => HandleRequestCraft(_currentTree));
            
            RefreshItems(ItemType.None, false);
        }

        private void InitCraftingItemUI(CraftTreeSO tree)
        {
            CraftItemUI ui = Instantiate(craftUIPrefab, root);

            ui.ItemButton.onClick.AddListener(() => HandleSelectTree(tree));
            ui.OnRequestCraft += HandleRequestCraft;
            ui.OnPinItem += HandlePinItem;
                
            ui.SetTree(tree);
            ui.RefreshUI(tree.Item, true);
                
            _itemDict.TryAdd(tree.Item, ui);
        }

        private void HandlePinItem(CraftItemUI ui, bool isPinned)
        {
            OnPinItem?.Invoke(ui, isPinned);
        }

        private void HandleRequestCraft(CraftTreeSO tree)
        {
            OnRequestCraft?.Invoke(tree);
        }

        private void HandleSelectTree(CraftTreeSO tree)
        {
            _currentTree = tree;
            OnTreeSelected?.Invoke(tree);
        }
        
        public void RefreshItems(ItemType itemType, bool isFavorite)
        {
            foreach (CraftItemUI ui in _itemDict.Values)
            {
                ui.DisableUI();
            }
            
            var query = _itemDict.AsEnumerable();
            
            if (itemType != ItemType.None)
            {
                query = query.Where(x => x.Key.itemType == itemType);
            }
            if (isFavorite)
            {
                query = query.Where(x => x.Value.IsFavorite);
            }
            
            query = query.OrderBy(x => x.Key.rarity);

            foreach (var ui in query)
            {
                ui.Value.EnableUI();
            }
        }

        private void OnDestroy()
        {
            foreach (CraftItemUI ui in _itemDict.Values)
            {
                ui.OnRequestCraft -= HandleRequestCraft;
                ui.ItemButton.onClick.RemoveAllListeners();
                ui.OnPinItem -= HandlePinItem;
            }
            
            createButton.onClick.RemoveAllListeners();
        }
    }
}