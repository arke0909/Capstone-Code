using Scripts.Entities;
using Work.LKW.Code.Items.ItemInfo;
using Chipmunk.ComponentContainers;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using UnityEngine;

namespace Work.LKW.Code.Items
{
    public class MedicineItem : UsableItem
    {
        public int HealAmount { get; set; }

        public MedicineItem(ItemDataSO itemData, int healAmount) : base(itemData)
        {
            HealAmount = healAmount;
        }
        
        public override void Use(Entity user)
        {
            if (user.TryGet<HealthCompo>(out var healthCompo))
                healthCompo.CurrentValue += HealAmount;
            
            base.Use(user);
        }
    }
}