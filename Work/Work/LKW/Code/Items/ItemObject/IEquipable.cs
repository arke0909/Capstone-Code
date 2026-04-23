using Work.LKW.Code.Items.ItemInfo;
using Scripts.Entities;
using UnityEngine;
using Work.SHS.Items.Events;

namespace Work.LKW.Code.Items
{
    public interface IEquipable
    {
        public EquipItemDataSO EquipItemData { get; }
        public bool IsEquipped { get; set; }
        public void OnEquip(Entity entity, Transform parent);
        public void OnUnequip(Entity entity);
    }

    public static class EquipableExtensions
    {
        public static void Equip(this IEquipable equipable, Entity entity, Transform parent)
        {
            if (equipable == null || equipable.IsEquipped) return;
            equipable.OnEquip(entity, parent);
            equipable.IsEquipped = true;
            entity.LocalEventBus.Raise(new ItemEquippedEvent(equipable));
        }

        public static void Unequip(this IEquipable equipable, Entity entity)
        {
            if (equipable == null || !equipable.IsEquipped) return;
            equipable.OnUnequip(entity);
            equipable.IsEquipped = false;
            entity.LocalEventBus.Raise(new ItemUnEquippedEvent(equipable));
        }
    }
}