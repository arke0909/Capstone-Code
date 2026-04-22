using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.StatusEffectSystem;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.Items
{
    public class StatusEffectItem : UsableItem
    {
        public StatusEffectItem(ItemDataSO itemData) : base(itemData)
        {
            StatusEffectData = itemData as StatusEffectItemDataSO;
            Debug.Assert(StatusEffectData != null, "StatusEffectData is null");
        }
        
        public StatusEffectItemDataSO StatusEffectData { get; private set; }

        public override void Use(Entity user)
        {
            foreach (var info in  StatusEffectData.buffs.GetStatusEffectInfo())
            {
                user.Get<EntityStatusEffect>().AddStatusEffect(info);
            }
            base.Use(user);
        }
    }
}