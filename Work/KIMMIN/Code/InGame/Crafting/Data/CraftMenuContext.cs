using Work.Code.Craft.Presenter;
using Work.Code.Craft.View;

namespace Work.Code.Craft
{
    public class CraftMenuContext
    {
        public CraftModel Model;
        public CraftMenuView MenuView;
        public CraftFilter Filter;
        public CraftPinController PinController;
        public CraftTreePresenter TreePresenter;
    }
}