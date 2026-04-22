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
            if (data.foodAmount != 0)
                BindStat(foodText, StatType.Hunger, data.foodAmount);
            if (data.waterAmount != 0)
                BindStat(waterText, StatType.Thirst, data.waterAmount);
        }
    }
}