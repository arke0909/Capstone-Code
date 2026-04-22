using TMPro;
using UnityEngine;
using Work.Code.UI.Utility;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.UI.Tooltip
{
    public abstract class ItemSlotTooltip<TData> : BaseTooltip<TData> where TData : ItemDataSO
    {
        [SerializeField] private StatColorSO statSettings;
        [SerializeField] private GameObject background;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        public override int SortOrder => 5;

        protected void BindStat(TextMeshProUGUI text, StatType type, float value)
        {
            var setting = statSettings.Get(type);
            text.text = ColorUtil.Format(setting, value);
        }
        
        protected sealed override void ShowTooltip(TData data)
        {
            background.SetActive(true);
            titleText.text = data.itemName;
            descriptionText.text = data.description;

            ShowData(data);
        }
        
        protected abstract void ShowData(TData data);
    }
}