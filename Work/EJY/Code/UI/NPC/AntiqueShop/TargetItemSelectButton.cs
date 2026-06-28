using System;
using Code.Items.ItemInfo;

namespace Code.UI.NPC
{
    public class TargetItemSelectButton : ItemSelectButton
    {
        public void Init(ItemDataSO itemData, Action<ItemDataSO> onSelect)
        {
            InitItem(itemData);
            BindSelect(onSelect);
            SetSelectedState(false);
        }
    }
}
