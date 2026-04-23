using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Crafting
{
    public class ItemDataUI : LayoutUIBase, IUIElement<ItemDataSO>
    {
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Image itemIcon;
        
        public void EnableFor(ItemDataSO item)
        {
            EnableUI();
            itemName.text = item.itemName;
            itemIcon.sprite = item.itemImage;
        }
        
        public void SetNameColor(Color32 color) => itemName.color = color;
        public void SetCountText(string text) => countText.text = text;

        public void Clear() { }
    }
}