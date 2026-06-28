using Code.Items.ItemInfo;
using Code.UI.Core;
using DewmoLib.Dependencies;
using Scripts.Players;
using System.Collections.Generic;
using UnityEngine;
using Work.Code.Craft.Presenter;
using Work.Code.Craft.View;
using Work.Code.GameEvents;

namespace Work.Code.Craft.Installer
{
    public class CraftTreeUI : UIPanel
    {
        [Inject] private Player _player;
        
        [SerializeField] private CraftMenuView menuView;
        [SerializeField] private CraftTreeView treeView;
        [SerializeField] private CraftFilter filter;
        [SerializeField] private CraftPinController pinController;

        [SerializeField] private CraftTreeListSO treeListSO;
        [SerializeField] private PlayerInputSO playerInput;
        
        private CraftMenuPresenter _menuPresenter;
        private CraftTreePresenter _treePresenter;
        private CraftModel _model;
        private readonly HashSet<ItemDataSO> _tutorialCraftItems = new();
        
        private void Start()
        {
            playerInput.OnCraftTreePressed += HandleToggleUI;
            _player.LocalEventBus.Subscribe<StartCraftingEvent>(HandleStartCrafting);
            _player.LocalEventBus.Subscribe<CompleteCraftingEvent>(HandleCompleteCrafting);
                
            _model = new CraftModel(_player);

            menuView.InitMenuView(treeListSO);
            treeView.InitTreeView(_model.Inventory);
            
            _treePresenter = new CraftTreePresenter(_model, treeView);

            CraftMenuContext context = new CraftMenuContext()
            {
                Model = _model,
                MenuView = menuView,
                Filter = filter,
                PinController = pinController,
                TreePresenter = _treePresenter
            };

            _menuPresenter = new CraftMenuPresenter(context);
            RefreshTutorialCraftItems();
        }
        
        private void HandleStartCrafting(StartCraftingEvent evt)
        {
            UIManager.Instance.SetLockState(true);
            DisableUI();
        }

        private void HandleCompleteCrafting(CompleteCraftingEvent evt)
        {
            UIManager.Instance.SetLockState(false);
            EnableUI();
        }

        private void HandleToggleUI()
        {
            ToggleUI(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            playerInput.OnCraftTreePressed -= HandleToggleUI;
            _menuPresenter.DisposePresenter();
            _treePresenter.DisposePresenter();
        }

        public void HighlightCraftItem(ItemDataSO item, bool isPlay, Color effectColor = default)
        {
            menuView.HighlightCraftItem(item, isPlay, effectColor);
        }

        public void SetTutorialItemType(ItemType itemType, Rarity itemRarity, Color effectColor)
        {
            menuView.SetTutorialItemType(itemType, itemRarity, effectColor);
        }

        public void ClearTutorialItemType()
        {
            menuView.ClearTutorialItemType();
            RefreshTutorialCraftItems();
        }

        public void RegisterTutorialCraftItem(ItemDataSO item)
        {
            if (item == null)
                return;

            _tutorialCraftItems.Add(item);
            RefreshTutorialCraftItems();
        }

        public void UnregisterTutorialCraftItem(ItemDataSO item)
        {
            if (item == null)
                return;

            _tutorialCraftItems.Remove(item);
            RefreshTutorialCraftItems();
        }

        private void RefreshTutorialCraftItems()
        {
            if (_tutorialCraftItems.Count == 0)
            {
                menuView.ClearInteractableItems();
                return;
            }

            menuView.SetInteractableItems(_tutorialCraftItems);
        }
    }
}
