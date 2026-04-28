using Code.DataSystem;
using Work.LKW.Code.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Work.LKW.Code.Items.ItemInfo
{
    [CreateAssetMenu(fileName = "FoodDataSO", menuName = "SO/Item/FoodData", order = 0)]
    public class FoodDataSO : UseItemDataSO
    {
        [ExcelColumn("staminaAmount")]
        public int staminaAmount;
        
        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new FoodItem(this, staminaAmount), Random.Range(1, maxSpawnCount));
        }
    }
}