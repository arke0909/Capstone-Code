using Code.DataSystem;
using Work.LKW.Code.Items;
using UnityEngine;

namespace Work.LKW.Code.Items.ItemInfo
{
    [CreateAssetMenu(fileName = "MedicineDataSO", menuName = "SO/Item/MedicineData", order = 0)]
    public class MedicineDataSO : UseItemDataSO
    {
        [ExcelColumn("healAmount")]
        public int healAmount;

        public override ItemCreateData CreateItem()
            => new ItemCreateData(new MedicineItem(this, healAmount), Random.Range(1, maxSpawnCount));
    }
}