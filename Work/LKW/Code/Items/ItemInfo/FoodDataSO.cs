using Code.DataSystem;
using Work.LKW.Code.Items;
using UnityEngine;

namespace Work.LKW.Code.Items.ItemInfo
{
    [CreateAssetMenu(fileName = "FoodDataSO", menuName = "SO/Item/FoodData", order = 0)]
    public class FoodDataSO : UseItemDataSO
    {
        [ExcelColumn("foodAmount")]
        public int foodAmount;
        [ExcelColumn("waterAmount")]
        public int waterAmount;
        
        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new FoodItem(this, foodAmount, waterAmount), Random.Range(1, maxSpawnCount));
        }
    }
}