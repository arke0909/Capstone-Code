using System;
using Code.InventorySystems;
using Scripts.Entities;
using Work.LKW.Code.Items.ItemInfo;
using UnityEngine;

namespace Work.LKW.Code.Items
{
    [Serializable]
    public abstract class ItemBase
    {
        protected Entity _owner; 

        [field: SerializeField] public ItemDataSO ItemData { get; protected set; }

        public ItemBase(ItemDataSO itemData)
        {
            ItemData = itemData;
        }
        
        public void SetOwner(Entity owner) => _owner = owner;
    }
}