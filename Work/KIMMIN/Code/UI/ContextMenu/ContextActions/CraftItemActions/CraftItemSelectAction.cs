using Work.Code.Craft;

namespace Work.Code.UI.ContextMenu
{
    public class CraftItemSelectAction : BaseContextAction<CraftItemUI>
    {
        public override bool CheckCondition(CraftItemUI data)
        {
            return true;
        }

        public override bool CanShow(CraftItemUI data)
        {
            return true;
        }

        public override void OnAction(CraftItemUI data)
        {
            data.RequestCraft();
        }
    }
}