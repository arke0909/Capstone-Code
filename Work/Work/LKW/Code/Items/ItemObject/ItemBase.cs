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
        [field: SerializeField] public ItemDataSO ItemData { get; protected set; }

        protected Entity _owner; 

        public ItemBase(ItemDataSO itemData)
        {
            ItemData = itemData;
        }
        
        public void SetOwner(Entity owner) => _owner = owner;
    }
}