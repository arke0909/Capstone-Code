using Work.Code.Craft.View;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft.Presenter
{
    public class CraftTreePresenter
    {
        private readonly CraftModel _model;
        private readonly CraftTreeView _treeView;
        
        public CraftTreePresenter(CraftModel craftModel, CraftTreeView treeView)
        {
            _model = craftModel;
            _treeView = treeView;

            _treeView.RequestItemCount += HandleGetItemCount;
            _treeView.OnNodeSelected += SelectTree;
        }

        private int HandleGetItemCount(ItemDataSO item)
        {
            return _model.Inventory.GetItemCount(item);
        }
        
        public void SelectTree(CraftTreeSO tree)
        {
            _treeView.RenderTree(tree, hasAnim: true);
        }

        public void DisposePresenter()
        {
            _treeView.RequestItemCount -= HandleGetItemCount;
            _treeView.OnNodeSelected -= SelectTree;
        }
    }
}