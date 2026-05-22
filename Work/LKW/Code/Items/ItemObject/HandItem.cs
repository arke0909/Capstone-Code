using Code.Items.ItemInfo;
using UnityEngine;

namespace Code.Items
{
    public abstract class HandItem : EquipableItem
    {
        protected HandItem(ItemDataSO itemData) : base(itemData)
        {
        }
    }
}