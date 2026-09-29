using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using DewmoLib.Dependencies;
using InGame.PlayerUI;
using Scripts.Players;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Craft.View
{
    public class CraftableItemsPanel : MonoBehaviour
    {
        [Inject] private Player _player;

        [SerializeField] private CraftItemUI itemPrefab;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private CraftTreeListSO treeListSO;
        [SerializeField, Min(0f)] private float itemIconPadding = 5f;

        private readonly Dictionary<CraftTreeSO, CraftItemUI> _itemUIs = new();
        private CraftModel _model;
        private PlayerInventory _inventoryPanel;

        private void Start()
        {
            _model = new CraftModel(_player);
            _inventoryPanel = GetComponentInParent<PlayerInventory>();

            CreateItemUIs();
            _model.Inventory.InventoryChanged += RefreshCraftableItems;
            RefreshCraftableItems();
        }

        private void CreateItemUIs()
        {
            if (itemPrefab == null || contentRoot == null || treeListSO == null)
                return;

            IEnumerable<CraftTreeSO> trees = treeListSO.list
                .Where(tree => tree != null && tree.Item != null)
                .OrderBy(tree => tree.Item.rarity);

            foreach (CraftTreeSO tree in trees)
            {
                if (_itemUIs.ContainsKey(tree))
                    continue;

                CraftItemUI itemUI = Instantiate(itemPrefab, contentRoot);
                itemUI.SetTree(tree);
                itemUI.RefreshUI(tree.Item, false);
                itemUI.SetIconPadding(itemIconPadding);
                itemUI.ItemButton.onClick.AddListener(() => HandleCraftRequest(tree));
                itemUI.OnRequestCraft += HandleCraftRequest;
                _itemUIs.Add(tree, itemUI);
            }
        }

        private void RefreshCraftableItems()
        {
            if (_model == null)
                return;

            foreach (var pair in _itemUIs)
            {
                bool canCraft = _model.CanCraft(pair.Key);
                pair.Value.SetInteractable(canCraft);

                if (canCraft)
                    pair.Value.EnableUI();
                else
                    pair.Value.DisableUI();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot as RectTransform);
        }

        private void HandleCraftRequest(CraftTreeSO tree)
        {
            if (tree == null || _model == null || !_model.CanCraft(tree))
                return;

            CraftRequestResult result = _model.TryCraft(tree);
            if (result != CraftRequestResult.Success)
            {
                RefreshCraftableItems();
                return;
            }

            _inventoryPanel?.DisableUI();
        }

        private void OnDestroy()
        {
            if (_model != null)
                _model.Inventory.InventoryChanged -= RefreshCraftableItems;

            foreach (CraftItemUI itemUI in _itemUIs.Values)
            {
                if (itemUI == null)
                    continue;

                itemUI.ItemButton.onClick.RemoveAllListeners();
                itemUI.OnRequestCraft -= HandleCraftRequest;
            }

            _itemUIs.Clear();
        }
    }
}
