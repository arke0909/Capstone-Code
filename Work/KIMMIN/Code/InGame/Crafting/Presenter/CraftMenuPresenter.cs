using UnityEngine;
using Work.Code.Craft.View;
using Code.Items.ItemInfo;
using Code.UI.Core;

namespace Work.Code.Craft.Presenter
{
    public class CraftMenuPresenter
    {
        private readonly CraftModel _model;
        private readonly CraftMenuView _menuView;
        private readonly CraftTreePresenter _treePresenter;
        private readonly CraftPinController _pinController;
        private readonly CraftFilter _filter;
        
        public CraftMenuPresenter(CraftMenuContext menuContext)
        {
            _model = menuContext.Model;
            _menuView = menuContext.MenuView;
            _filter = menuContext.Filter;
            _pinController = menuContext.PinController;
            _treePresenter = menuContext.TreePresenter;
            
            _menuView.OnRequestCraft += HandleRequestCraft;
            _menuView.OnTreeSelected += HandleTreeSelected;
            _filter.OnRefreshCraftUI += HandleRefreshCraftUI;
            _menuView.OnPinItem += HandlePinItem;
            _treePresenter.OnTreeSelected += _menuView.SetCurrentTree;
            _model.Inventory.InventoryChanged += HandleInventoryChanged;

            HandleInventoryChanged();
        }

        private void HandleInventoryChanged()
        {
            _menuView.RefreshCraftableItems(_model.CanCraft, UIDefine.GreenColor);
        }

        private void HandlePinItem(CraftItemUI ui, bool isPinned)
        {
            _pinController.ModifyPin(ui, isPinned);
        }

        private void HandleRefreshCraftUI(ItemType[] types, bool isFavorite)
        {
            _menuView.RefreshItems(types, isFavorite);
        }

        private void HandleTreeSelected(CraftTreeSO tree)
        {
            _treePresenter.SelectTree(tree);
        }

        private void HandleRequestCraft(CraftTreeSO tree)
        {
            CraftRequestResult result = _model.TryCraft(tree);
            _menuView.SetInventoryFullText(result == CraftRequestResult.InventoryFull);
        }
        
        public void DisposePresenter()
        {
            _filter.OnRefreshCraftUI -= HandleRefreshCraftUI;
            _menuView.OnRequestCraft -= HandleRequestCraft;
            _menuView.OnTreeSelected -= HandleTreeSelected;
            _menuView.OnPinItem -= HandlePinItem;
            _treePresenter.OnTreeSelected -= _menuView.SetCurrentTree;
            _model.Inventory.InventoryChanged -= HandleInventoryChanged;
        }
    }
}
