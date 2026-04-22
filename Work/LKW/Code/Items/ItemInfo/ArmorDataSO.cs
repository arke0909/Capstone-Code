using Code.DataSystem;
using Code.InventorySystems.Items;
using Work.LKW.Code.Items;
using UnityEngine;

namespace Work.LKW.Code.Items.ItemInfo
{
    
    [CreateAssetMenu(fileName = "ArmorDataSO", menuName = "SO/Item/ArmorData", order = 0)]
    public class ArmorDataSO : EquipItemDataSO
    {
        [ExcelColumn("maxDurability")]
        public int maxDurability;
        
        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new ArmorItem(this, maxDurability), Random.Range(1, maxSpawnCount));
        }
    }
}