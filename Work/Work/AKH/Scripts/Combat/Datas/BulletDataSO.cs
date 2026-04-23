using Code.DataSystem;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    [CreateAssetMenu(fileName = "BulletDataSO", menuName = "SO/Item/BulletData", order = 0)]
    public class BulletDataSO : ItemDataSO
    {
        [ExcelColumn("gunType")]
        public GunType gunType;
        [ExcelColumn("damageMultiplier")]
        public float damageMultiplier = 1;
        [ExcelColumn("defPierceLevel")]
        public int defPierceLevel = 0;
        public override ItemCreateData CreateItem()
        {
            return new ItemCreateData(new BulletItem(this), maxSpawnCount);
        }
    }
}
