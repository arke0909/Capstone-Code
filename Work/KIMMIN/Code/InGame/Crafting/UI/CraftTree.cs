using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using InGame.PlayerUI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Work.Code.GameEvents;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Crafting
{
    public class CraftTree : UIPanel
    {
        [SerializeField] private CraftTreeListSO tree;
        [SerializeField] private CraftItemUI itemUIPrefab;
        [SerializeField] private CraftTreeUI treeUI;
        [SerializeField] private Transform root;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private PinCraftItem pinItemUI;
        [SerializeField] private CraftController craftController;

        [SerializeField] private FavoriteSelectUI unfavoriteButton;
        [SerializeField] private FavoriteSelectUI favoriteButton;
        [SerializeField] private TMP_Dropdown sortDropdown;
        
        [SerializeField] private PlayerInputSO playerInput;
        [SerializeField] private GameObject craftUI;
        
        private MapViewButton _mapViewButton;
        private CraftTypeUI[] _craftTypes;
        private Dictionary<ItemDataSO, CraftItemUI> _items = new();
        private Dictionary<ItemDataSO, CraftTreeSO> _treeLookup = new();
        private Queue<CraftItemUI> _pinQueue = new();
        
        private CraftTreeSO _prevTree;
        private ItemType _type = ItemType.None;
        private bool _isFavorite = false;

        protected override void Awake()
        {
            base.Awake();
            
            Init();
            InitUI();
            HandleUnFavorite();
        }

        private void Start()
        {
            treeUI.Initialize(craftController);
        }

        private void Init()
        {
            _craftTypes = GetComponentsInChildren<CraftTypeUI>();
            _mapViewButton = GetComponentInChildren<MapViewButton>();
            
            foreach (var craftType in _craftTypes)
            {
                craftType.OnItemSelected += HandleSelectType;
            }
            
            _mapViewButton.OnShowItems += HandleViewItems;
            playerInput.OnCraftTreePressed += HandleCraftTreePressed;
            favoriteButton.SelectButton.onClick.AddListener(HandleFavorite);
            unfavoriteButton.SelectButton.onClick.AddListener(HandleUnFavorite);
            sortDropdown.onValueChanged.AddListener(HandleSortSelect);
        }

        private void InitUI()
        {
            foreach (var node in tree.list)
            {
                if (node.Item == null) continue;
                
                var ui = Instantiate(itemUIPrefab, root).GetComponent<CraftItemUI>();
                _items.TryAdd(node.Item, ui);
                _treeLookup.TryAdd(node.Item, node);
                
                ui.DisableUI();
                ui.ItemButton.onClick.AddListener(() => HandleClickItem(node));
                ui.OnPinItem += HandlePinItem;
                ui.OnRequestCraft += HandleTryCraft;
            }
        }

        private void HandleTryCraft(CraftTreeSO tree)
        {
            treeUI.CreateItem(tree);
        }

        private void HandlePinItem(CraftItemUI item, bool isPinned)
        {
            if (isPinned)
            {
                if (_pinQueue.Count >= 3)
                {
                    var oldest = _pinQueue.Dequeue();
                    oldest.SetPin(false);
                    pinItemUI.PopItem(oldest.Tree);
                }

                _pinQueue.Enqueue(item);
                item.SetPin(true);
                pinItemUI.PushItem(item.Tree);
            }
            else
            {
                var newQueue = new Queue<CraftItemUI>();
                while (_pinQueue.Count > 0)
                {
                    var qItem = _pinQueue.Dequeue();
                    if (qItem != item)
                        newQueue.Enqueue(qItem);
                }

                _pinQueue = newQueue;

                item.SetPin(false);
                pinItemUI.PopItem(item.Tree);
            }
        }
        
        private void HandleCraftTreePressed() => ToggleUI(true);

        private void HandleViewItems()
        {
            if (_prevTree == null) return;

            List<ItemDataSO> itemsToShow = _prevTree.nodeList
                .Select(item => item.Item)
                .ToList();
            
            itemsToShow.RemoveAt(0);
            EventBus.Raise(new ShowItemsOnMap(itemsToShow));
            ToggleUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var craftType in _craftTypes)
            {
                craftType.OnItemSelected -= HandleSelectType;
            }

            foreach (var node in tree.list)
            {
                _items[node.Item].ItemButton.onClick.RemoveAllListeners();
                _items[node.Item].OnPinItem -= HandlePinItem;
            }
            
            _mapViewButton.OnShowItems -= HandleViewItems;
            playerInput.OnCraftTreePressed -= HandleCraftTreePressed;
            favoriteButton.SelectButton.onClick.RemoveListener(HandleFavorite);
            unfavoriteButton.SelectButton.onClick.RemoveListener(HandleUnFavorite);
            sortDropdown.onValueChanged.RemoveListener(HandleSortSelect);
        }

        private void HandleSortSelect(int index)
        {
            RefreshItems();
        }

        private void HandleUnFavorite()
        {
            SetFavoriteState(false);
        }

        private void HandleFavorite()
        {
            SetFavoriteState(true);
        }
        
        private void SetFavoriteState(bool state)
        {
            _isFavorite = state;
            favoriteButton.OnSelect(state);
            unfavoriteButton.OnSelect(!state);
            RefreshItems();
        }

        private void HandleSelectType(ItemType type)
        {
            if (_type == type)
            {
                _type = ItemType.None;
                typeText.text = "전체 아이템";
                RefreshItems();
                return;
            }
            
            _type = type;
            typeText.text = type.ToString();
            RefreshItems();
        }
        
        private void RefreshItems(bool isNotificate = true)
        {
            ToggleItems(false);

            var query = _items.AsEnumerable();
            if (_type != ItemType.None) query = query.Where(x => x.Key.itemType == _type);
            if (_isFavorite) query = query.Where(x => x.Value.IsFavorite);

            query = sortDropdown.value switch
            {
                0 => query.OrderBy(x => x.Key.rarity),
                1 => query.OrderBy(x => x.Key.itemName),
                _ => query
            };

            int idx = 0;
            foreach (var pair in query)
            {
                var ui = pair.Value;
                ui.transform.SetSiblingIndex(idx++);
                ui.SetTree(_treeLookup[pair.Key]);
                ui.Init(pair.Key, craftController, isNotificate);
            }
        }

        public override void EnableUI(bool isFade = false)
        {
            base.EnableUI(isFade);
            ToggleItems(true);
        }

        public override void DisableUI(bool isFade = false)
        {
            base.DisableUI(isFade);
            ToggleItems(false);
        }

        private void ToggleItems(bool isActive)
        {
            foreach (var item in _items.Values)
            {
                if (isActive)
                    item.EnableUI();
                else
                    item.DisableUI();
            }
        }

        private void HandleClickItem(CraftTreeSO treeSO)
        {
            _prevTree = _prevTree == treeSO ? null : treeSO;
            treeUI.EnableFor(_prevTree);
        }
    }
}