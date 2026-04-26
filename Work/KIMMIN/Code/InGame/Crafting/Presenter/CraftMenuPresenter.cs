using UnityEngine;
using Work.Code.Craft.View;
using Work.LKW.Code.Items.ItemInfo;

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
        }

        private void HandlePinItem(CraftItemUI ui, bool isPinned)
        {
            _pinController.ModifyPin(ui, isPinned);
        }

        private void HandleRefreshCraftUI(ItemType type, bool isFavorite)
        {
            _menuView.RefreshItems(type, isFavorite);
        }

        private void HandleTreeSelected(CraftTreeSO tree)
        {
            _treePresenter.SelectTree(tree);
        }

        private void HandleRequestCraft(CraftTreeSO tree)
        {
            if (!_model.TryCraft(tree))
            {
                Debug.Log($"{tree.Item.itemName} 재료 부족");
            }
        }
        
        public void DisposePresenter()
        {
            _filter.OnRefreshCraftUI -= HandleRefreshCraftUI;
            _menuView.OnRequestCraft -= HandleRequestCraft;
            _menuView.OnTreeSelected -= HandleTreeSelected;
            _menuView.OnPinItem -= HandlePinItem;
        }
    }
}