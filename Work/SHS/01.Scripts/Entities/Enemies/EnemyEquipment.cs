using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.EnemySpawn;
using Code.GameEvents;
using Code.InventorySystems;
using Code.InventorySystems.Equipments;
using Code.Items;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.Combat.Datas;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public class EnemyEquipment : EntityEquipment, ILocalEventSubscriber<EnemySpawnEvent>
    {
        public bool IsInitialized => ComponentContainer != null;

        private Inventory _enemyInventory;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _enemyInventory = componentContainer.GetSubclassComponent<Inventory>();

            if (equipTrms == null)
                equipTrms = new SerializedDictionary<EquipPartType, Transform>();

            Transform parent = Owner != null ? Owner.transform : transform;
            for (int i = 0; i < (int)EquipPartType.Count; ++i)
            {
                EquipPartType partType = (EquipPartType)i;

                if (equipTrms.ContainsKey(partType) && equipTrms[partType] != null)
                    continue;

                var go = new GameObject($"Equip_TRM_{partType}");
                go.transform.SetParent(parent, false);
                equipTrms[partType] = go.transform;
            }
        }

        public void OnLocalEvent(EnemySpawnEvent spawnEvent)
        {
            ResetRuntimeEquipment();
            if (spawnEvent.EnemyData == null)
                return;

            EquipSpawnItems(spawnEvent.EnemyData.equipments);
        }

        public void ResetRuntimeEquipment()
        {
            for (int i = 0; i < (int)EquipPartType.Count; ++i)
            {
                UnequipItemFromPart((EquipPartType)i, out _);
            }

            _enemyInventory?.ClearInventory();
        }

        private void EquipSpawnItems(EnemyEquipData[] equipments)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("EnemyEquipment is not initialized yet!");
                return;
            }

            if (equipments == null) return;

            foreach (var equipData in equipments)
            {
                if (equipData.itemData == null) continue;

                EquipableItem item = equipData.itemData.CreateItem().Item as EquipableItem;

                if (item is ThrowableItem)
                {
                    _enemyInventory.TryAddItem(item, 1);
                }

                if (item != null)
                {
                    EquipItemToPart(equipData.partType, item);
                }
            }
        }

        protected override void OnItemEquipped(EquipPartType partType, EquipableItem item, Transform parent)
        {
            if (item is not HandItem handItem)
                return;

            if (handItem.ItemObject == null)
                handItem.Handle(Owner, parent);

            Owner?.LocalEventBus.Raise(new ChangeHandlingEvent(item));
        }

        protected override void OnItemUnequipped(EquipPartType partType, EquipableItem item)
        {
            if (item is not HandItem handItem)
                return;

            Owner?.LocalEventBus.Raise(new ChangeHandlingEvent(null));

            if (handItem.ItemObject != null)
                handItem.UnHandle(Owner);
        }
    }
}
