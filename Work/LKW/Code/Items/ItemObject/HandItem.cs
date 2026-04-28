using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.Items
{
    public abstract class HandItem : EquipableItem
    {
        protected HandItem(ItemDataSO itemData) : base(itemData)
        {
        }
    }
}