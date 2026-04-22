using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems;
using Work.LKW.Code.Items.ItemInfo;
using Scripts.Entities;
using UnityEngine;
using Work.Code.GameEvents;

namespace Work.LKW.Code.Items
{
    public abstract class UsableItem : Weapon, IUsable
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