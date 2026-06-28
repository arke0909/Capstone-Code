using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Code.Items.ItemInfo;

namespace Work.Code.Craft.View
{
    public class CraftMenuView : MonoBehaviour
    { 
        [FormerlySerializedAs("craftingUIPrefab")] [SerializeField] private CraftItemUI craftUIPrefab;
        [SerializeField] private Transform root; 
        [SerializeField] private Button createButton;
        [SerializeField] private GameObject inventoryFullText;

        private readonly Dictionary<ItemDataSO, CraftItemUI> _itemDict = new();
        private HashSet<ItemDataSO> _interactableItems;
        private Coroutine _inventoryFullTextRoutine;
        private bool _hasTutorialItemType;
        private ItemType _tutorialItemType = ItemType.None;
        private Rarity _tutorialRarity = Rarity.None;
        private Color _tutorialItemColor;
        private const float InventoryFullTextDuration = 3f;
        
        public event Action<CraftTreeSO> OnTreeSelected;
        public event Action<CraftTreeSO> OnRequestCraft;
        public event Action<CraftItemUI, bool> OnPinItem;

        private CraftTreeSO _currentTree;

        public void InitMenuView(CraftTreeListSO craftTreeList)
        {
            SetInventoryFullText(false);

            foreach (CraftTreeSO tree in craftTreeList.list)
            {
                if (tree.Item != null)
                {
                    InitCraftingItemUI(tree);
                }
            }
            
            createButton.onClick.AddListener(() => HandleRequestCraft(_currentTree));
            
            RefreshItems(null, false);
        }

        private void InitCraftingItemUI(CraftTreeSO tree)
        {
            CraftItemUI ui = Instantiate(craftUIPrefab, root);

            ui.ItemButton.onClick.AddListener(() => HandleSelectTree(tree));
            ui.OnRequestCraft += HandleRequestCraft;
            ui.OnPinItem += HandlePinItem;
                
            ui.SetTree(tree);
            ui.RefreshUI(tree.Item, true);
            ui.SetInteractable(CanInteract(tree.Item));
                
            _itemDict.TryAdd(tree.Item, ui);
        }

        private void HandlePinItem(CraftItemUI ui, bool isPinned)
        {
            OnPinItem?.Invoke(ui, isPinned);
        }

        private void HandleRequestCraft(CraftTreeSO tree)
        {
            if (tree == null || !CanInteract(tree.Item))
                return;

            SetInventoryFullText(false);
            OnRequestCraft?.Invoke(tree);
        }

        private void HandleSelectTree(CraftTreeSO tree)
        {
            SetCurrentTree(tree);
            OnTreeSelected?.Invoke(tree);
        }

        public void SetCurrentTree(CraftTreeSO tree)
        {
            _currentTree = tree;
            createButton.interactable = CanInteract(tree.Item);
            SetInventoryFullText(false);
        }
        
        public void RefreshItems(ItemType[] itemTypes, bool isFavorite)
        {
            foreach (CraftItemUI ui in _itemDict.Values)
            {
                ui.DisableUI();
            }
            
            var query = _itemDict.AsEnumerable();
            
            if (itemTypes != null && itemTypes.Length > 0)
            {
                query = query.Where(x => itemTypes.Contains(x.Key.itemType));
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

            RefreshTutorialItemType();

            if (!_hasTutorialItemType)
                RefreshInteractableItems();
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

        public void HighlightCraftItem(ItemDataSO item, bool isPlay, Color effectColor)
        {
            if (!_itemDict.TryGetValue(item, out CraftItemUI ui))
                return;
            
            if(isPlay)
                ui.PlayBackgroundEffect(effectColor);
            else
                ui.StopBackgroundEffect();
        }

        public void RefreshCraftableItems(Func<CraftTreeSO, bool> canCraft, Color effectColor)
        {
            foreach (CraftItemUI ui in _itemDict.Values)
            {
                ui.RefreshCraftableEffect(canCraft(ui.Tree), effectColor);
            }
        }

        public void SetInventoryFullText(bool isActive)
        {
            if (inventoryFullText == null)
                return;

            if (_inventoryFullTextRoutine != null)
            {
                StopCoroutine(_inventoryFullTextRoutine);
                _inventoryFullTextRoutine = null;
            }

            inventoryFullText.SetActive(isActive);

            if (isActive)
                _inventoryFullTextRoutine = StartCoroutine(HideInventoryFullTextRoutine());
        }

        private IEnumerator HideInventoryFullTextRoutine()
        {
            yield return new WaitForSeconds(InventoryFullTextDuration);
            inventoryFullText.SetActive(false);
            _inventoryFullTextRoutine = null;
        }

        public void SetInteractableItems(IReadOnlyCollection<ItemDataSO> items)
        {
            if (items == null || items.Count == 0)
            {
                ClearInteractableItems();
                return;
            }

            _interactableItems = new HashSet<ItemDataSO>(items);
            RefreshInteractableItems();
        }

        public void ClearInteractableItems()
        {
            _interactableItems = null;
            RefreshInteractableItems();
        }

        public void SetTutorialItemType(ItemType itemType, Rarity itemRarity, Color effectColor)
        {
            _hasTutorialItemType = true;
            _tutorialItemType = itemType;
            _tutorialItemColor = effectColor;
            _tutorialRarity = itemRarity;
            RefreshTutorialItemType();
        }

        public void ClearTutorialItemType()
        {
            _hasTutorialItemType = false;

            foreach (CraftItemUI ui in _itemDict.Values)
            {
                ui.StopBackgroundEffect();
            }

            ClearInteractableItems();
        }

        private void RefreshTutorialItemType()
        {
            if (!_hasTutorialItemType)
                return;

            List<ItemDataSO> items = new List<ItemDataSO>();

            foreach (var pair in _itemDict)
            {
                if (pair.Key.itemType == _tutorialItemType)
                {
                    if(_tutorialRarity != Rarity.None 
                       && _tutorialRarity != pair.Key.rarity)
                        continue;
                    
                    items.Add(pair.Key);
                    pair.Value.PlayBackgroundEffect(_tutorialItemColor);
                    continue;
                }

                pair.Value.StopBackgroundEffect();
            }

            SetInteractableItems(items);
        }

        private void RefreshInteractableItems()
        {
            foreach (var pair in _itemDict)
            {
                pair.Value.SetInteractable(CanInteract(pair.Key));
            }

            createButton.interactable = _currentTree != null && CanInteract(_currentTree.Item);
        }

        private bool CanInteract(ItemDataSO item)
        {
            return _interactableItems == null || _interactableItems.Contains(item);
        }
    }
}
