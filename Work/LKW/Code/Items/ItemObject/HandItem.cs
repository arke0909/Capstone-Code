using Code.Items.ItemInfo;
using Scripts.Entities;
using UnityEngine;

namespace Code.Items
{
    public abstract class HandItem : EquipableItem
    {
        protected HandItem(ItemDataSO itemData) : base(itemData)
        {
        }

        public virtual void Handle(Entity entity, Transform parent)
        {
            InitItemObject(entity, parent);            
        }

        public virtual void UnHandle(Entity entity)
        {
            DestroyItemObject();
        }
    }
}