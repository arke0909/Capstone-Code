using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftableItemUI : LayoutUIBase
    {
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private Image icon;
        
        public void EnableCraftableItemUI(ItemDataSO item)
        {
            EnableUI();
            itemText.text = item.itemName;
            icon.sprite = item.itemImage;
        }
    }
}