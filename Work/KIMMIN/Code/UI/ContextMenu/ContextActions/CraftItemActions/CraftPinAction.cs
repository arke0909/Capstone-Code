using Work.Code.Craft;

namespace Work.Code.UI.ContextMenu
{
    public class CraftPinAction : BaseContextAction<CraftItemUI>
    {
        public override bool CheckCondition(CraftItemUI data)
        {
            return data.IsPinned;
        }

        public override void OnAction(CraftItemUI data)
        {
            data.TogglePin();
        }
    }
}