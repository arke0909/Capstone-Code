using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Code.InventorySystems.Items;
using Code.Items;
using Scripts.Entities;
using UnityEngine;

namespace Code.InventorySystems.Equipments
{
    public abstract class EntityEquipment : MonoBehaviour, IContainerComponent
    {
        [SerializeField] protected SerializedDictionary<EquipPartType, Transform> equipTrms;

        private readonly Dictionary<EquipPartType, EquipableItem> _equips =
            new Dictionary<EquipPartType, EquipableItem>();

        protected Entity Owner { get; private set; }
        public ComponentContainer ComponentContainer { get; set; }

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            Owner = componentContainer.GetSubclassComponent<Entity>();
            _equips.Clear();

            for (int i = 0; i < (int)EquipPartType.Count; ++i)
            {
                _equips.Add((EquipPartType)i, null);
            }
        }

        public bool TryGetEquippedItem(EquipPartType partType, out EquipableItem item)
        {
            item = GetEquippedItem(partType);
            return item != null;
        }

        public EquipableItem GetEquippedItem(EquipPartType partType) => _equips.GetValueOrDefault(partType);

        public Transform GetEquipTransform(EquipPartType partType) => equipTrms.GetValueOrDefault(partType);

        public bool EquipItemToPart(EquipPartType partType, EquipableItem item)
        {
            if (item == null ||
                item.EquipItemData == null ||
                !_equips.ContainsKey(partType) ||
                item.EquipItemData.itemType.GetEquipSlotType().GetEquipType() != partType)
                return false;

            EquipableItem currentItem = GetEquippedItem(partType);
            Transform equipParent = GetEquipTransform(partType);
            if (equipParent == null)
                equipParent = Owner != null ? Owner.transform : transform;

            if (currentItem == item)
            {
                if (!item.IsEquipped)
                {
                    item.Equip(Owner, equipParent);
                    OnItemEquipped(partType, item, equipParent);
                }

                return true;
            }

            if (currentItem != null && !UnequipItemFromPart(partType, out _))
                return false;

            _equips[partType] = item;
            item.Equip(Owner, equipParent);
            OnItemEquipped(partType, item, equipParent);
            return true;
        }

        public bool UnequipItemFromPart(EquipPartType partType, out EquipableItem unequippedItem)
        {
            unequippedItem = GetEquippedItem(partType);
            if (unequippedItem == null)
                return false;

            OnItemUnequipped(partType, unequippedItem);
            unequippedItem.Unequip(Owner);
            _equips[partType] = null;
            return true;
        }

        protected virtual void OnItemEquipped(EquipPartType partType, EquipableItem item, Transform parent)
        {
        }

        protected virtual void OnItemUnequipped(EquipPartType partType, EquipableItem item)
        {
        }
    }
}
