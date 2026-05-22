using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems;
using Code.Items.ItemInfo;
using Scripts.Entities;
using UnityEngine;
using Work.Code.GameEvents;

namespace Code.Items
{
    public abstract class UsableItem : HandItem, IUsable
    {
        public UsableItem(ItemDataSO itemData) : base(itemData)
        {
        }


        // 임시
        public virtual void Use(Entity user)
        {
        }
    }
}