using System;
using Code.Items.ItemInfo;
using Work.Code.Craft.View;

namespace Work.Code.Craft.Presenter
{
    public class CraftTreePresenter
    {
        private readonly CraftModel _model;
        private readonly CraftTreeView _treeView;
        private CraftTreeSO _currentTree;

        public event Action<CraftTreeSO> OnTreeSelected;
        
        public CraftTreePresenter(CraftModel craftModel, CraftTreeView treeView)
        {
            _model = craftModel;
            _treeView = treeView;

            _treeView.RequestItemCount += HandleGetItemCount;
            _treeView.RequestCanCraftTree += HandleCanCraftTree;
            _treeView.OnNodeSelected += SelectTree;
            _model.Inventory.InventoryChanged += HandleInventoryChanged;
        }

        private int HandleGetItemCount(ItemDataSO item)
        {
            return _model.Inventory.GetItemCount(item);
        }

        private bool HandleCanCraftTree(CraftTreeSO tree)
        {
            return _model.CanCraft(tree);
        }
        
        public void SelectTree(CraftTreeSO tree)
        {
            _currentTree = tree;
            _treeView.RenderTree(tree, hasAnim: true);
            OnTreeSelected?.Invoke(tree);
        }

        private void HandleInventoryChanged()
        {
            if (_currentTree == null)
                return;

            _treeView.RenderTree(_currentTree, hasAnim: false);
        }

        public void DisposePresenter()
        {
            _treeView.RequestItemCount -= HandleGetItemCount;
            _treeView.RequestCanCraftTree -= HandleCanCraftTree;
            _treeView.OnNodeSelected -= SelectTree;
            _model.Inventory.InventoryChanged -= HandleInventoryChanged;
        }
    }
}
