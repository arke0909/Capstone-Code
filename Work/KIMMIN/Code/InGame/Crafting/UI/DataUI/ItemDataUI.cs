using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class ItemDataUI : LayoutUIBase
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image itemIcon;
        
        public void EnableDataUI(ItemDataSO item)
        {
            EnableUI();
            itemName.text = item.itemName;
            itemIcon.sprite = item.itemImage;
        }
        
        public void SetNameColor(Color32 color) => itemName.color = color;
        public void SetCountText(string text) => countText.text = text;
    }
}