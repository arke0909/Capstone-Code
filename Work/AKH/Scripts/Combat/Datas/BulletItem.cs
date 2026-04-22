using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Combat.Datas
{
    public class BulletItem : ItemBase
    {
        public BulletItem(ItemDataSO itemData) : base(itemData)
        {
            Debug.Assert(itemData is BulletDataSO, "Invalid Type Data");
            bulletDataSO = itemData as BulletDataSO;
        }
        public BulletDataSO bulletDataSO { get; private set; }
    }
}
