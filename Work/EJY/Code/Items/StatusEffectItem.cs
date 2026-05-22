using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Items.ItemInfo;
using Code.StatusEffectSystem;
using Scripts.Entities;
using UnityEngine;
using Code.Items;

namespace Code.Items
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
            user.Get<EntityStatusEffect>().AddStatusEffect(StatusEffectData.buffs.GetStatusEffectInfo());
            base.Use(user);
        }
    }
}