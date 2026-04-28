using TMPro;
using UnityEngine;
using Work.Code.UI.Utility;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.UI.Tooltip
{
    public class FoodItemTooltip : ItemSlotTooltip<FoodDataSO>
    {
        [SerializeField] private TextMeshProUGUI foodText;
        [SerializeField] private TextMeshProUGUI waterText;
            
        protected override void ShowData(FoodDataSO data)
        {
            if (data.staminaAmount != 0)
                BindStat(foodText, StatType.Hunger, data.staminaAmount);
        }
    }
}