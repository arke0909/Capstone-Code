using Code.Items.ItemInfo;
using Scripts.Entities;
using UnityEngine;

namespace Code.Items
{
    public class ArmorItem : EquipableItem
    {
        public int MaxDurability { get; set; }

        public ArmorItem(ItemDataSO itemData, int maxDurability) : base(itemData)
        {
            MaxDurability = maxDurability;
        }

        public override void OnEquip(Entity entity, Transform parent)
        {
            base.OnEquip(entity, parent);
            InitItemObject(entity, parent);
        }

        public override void OnUnequip(Entity entity)
        {
            base.OnUnequip(entity);
            DestroyItemObject();
        }
    }
}