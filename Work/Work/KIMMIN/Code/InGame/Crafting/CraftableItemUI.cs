using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Crafting
{
    public class CraftableItemUI : LayoutUIBase, IUIElement<ItemDataSO>
    {
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private Image icon;
        
        public void EnableFor(ItemDataSO item)
        {
            EnableUI();
            itemText.text = item.itemName;
            icon.sprite = item.itemImage;
        }

        public void Clear()
        {
            DisableUI();
        }
    }
}