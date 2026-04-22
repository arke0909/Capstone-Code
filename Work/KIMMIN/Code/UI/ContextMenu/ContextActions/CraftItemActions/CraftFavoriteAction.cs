using UnityEngine;
using Work.Code.Crafting;

namespace Work.Code.UI.ContextMenu
{
    public class CraftFavoriteAction : BaseContextAction<CraftItemUI>
    {
        public override bool CheckCondition(CraftItemUI data)
        {
            return data.IsFavorite;
        }

        public override void OnAction(CraftItemUI data)
        {
            data.ToggleFavorite();
        }
    }
}