using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Chipmunk.Modules.StatSystem;
using Code.GameEvents;
using Code.InventorySystems;
using Code.InventorySystems.Items;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;

namespace Code.Players
{
    public class PlayerInventory : Inventory,IAfterInitialze
    {
        [SerializeField] private StatSO invenSlotCountStat;
        [SerializeField] private PoolItemSO previewItem;
        [Inject] private PoolManagerMono _poolManagerMono;
        private StatOverrideBehavior _StatOverrideBehavior;
        private PlayerEquipment _playerEquipment;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _StatOverrideBehavior = ComponentContainer.Get<StatOverrideBehavior>();
            _playerEquipment = componentContainer.Get<PlayerEquipment>();

            InventoryChanged += UpdateUI;
        }

        public void AfterInitialize()
        {
            CurrentInventorySize =
                (int)_StatOverrideBehavior.SubscribeStat(invenSlotCountStat, HandleInvenSlotCount, CurrentInventorySize);
            UpdateInventory();
        }
        
        private struct SimulatedSlot
        {
            public ItemDataSO ItemData;
            public int Stack;
        
            public SimulatedSlot(ItemDataSO itemData, int stack)
            {
                ItemData = itemData;
                Stack = stack;
            }
        
            public bool IsBlank => ItemData == null || Stack <= 0;
        }
        
        public bool CanAddItemAfterConsume(ItemDataSO resultItem, int resultCount,
            Dictionary<ItemDataSO, int> consumeItems)
        {
            if (resultItem == null || resultCount <= 0)
                return false;
        
            List<SimulatedSlot> simulatedSlots = new List<SimulatedSlot>(CurrentInventorySize);
        
            for (int i = 0; i < CurrentInventorySize; i++)
            {
                ItemSlot slot = itemSlots[i];
                simulatedSlots.Add(new SimulatedSlot(slot.Item?.ItemData, slot.Stack));
            }
        
            if (consumeItems != null)
            {
                foreach (var pair in consumeItems)
                {
                    if (pair.Key == null || pair.Value <= 0)
                        continue;
        
                    int remainingToConsume = pair.Value;
        
                    for (int i = 0; i < simulatedSlots.Count && remainingToConsume > 0; i++)
                    {
                        SimulatedSlot slot = simulatedSlots[i];
        
                        if (slot.ItemData != pair.Key || slot.Stack <= 0)
                            continue;
        
                        int removed = Mathf.Min(slot.Stack, remainingToConsume);
                        slot.Stack -= removed;
                        remainingToConsume -= removed;
        
                        if (slot.Stack <= 0)
                        {
                            slot.ItemData = null;
                            slot.Stack = 0;
                        }
        
                        simulatedSlots[i] = slot;
                    }

                    if (remainingToConsume > GetEquippedItemCount(pair.Key))
                        return false;
                }
            }
        
            int addableCount = 0;
            int maxStack = resultItem.maxStack;
        
            for (int i = 0; i < simulatedSlots.Count; i++)
            {
                SimulatedSlot slot = simulatedSlots[i];
        
                if (slot.IsBlank)
                {
                    addableCount += maxStack;
                }
                else if (slot.ItemData == resultItem)
                {
                    addableCount += maxStack - slot.Stack;
                }
        
                if (addableCount >= resultCount)
                    return true;
            }

            return false;
        }

        public int GetCraftAvailableItemCount(ItemDataSO itemData)
        {
            return GetItemCount(itemData) + GetEquippedItemCount(itemData);
        }

        public bool CanConsumeCraftMaterials(Dictionary<ItemDataSO, int> consumeItems)
        {
            if (consumeItems == null)
                return false;

            foreach (var pair in consumeItems)
            {
                if (GetCraftAvailableItemCount(pair.Key) < pair.Value)
                    return false;
            }

            return true;
        }

        public bool TryConsumeCraftMaterials(Dictionary<ItemDataSO, int> consumeItems)
        {
            if (!CanConsumeCraftMaterials(consumeItems))
                return false;

            foreach (var pair in consumeItems)
            {
                int remaining = pair.Value;
                int inventoryCount = GetItemCount(pair.Key);

                if (inventoryCount > 0)
                {
                    int inventoryConsumeCount = Mathf.Min(inventoryCount, remaining);

                    if (!RemoveItemByData(pair.Key, inventoryConsumeCount))
                        return false;

                    remaining -= inventoryConsumeCount;
                }

                if (remaining > 0 && (_playerEquipment == null || !_playerEquipment.TryConsumeEquippedItems(pair.Key, remaining)))
                    return false;
            }

            return true;
        }

        public EquipableItem FindCraftSkillSourceItem(ItemDataSO itemData)
        {
            foreach (ItemSlot itemSlot in GetItemSlots(itemData))
            {
                if (itemSlot.Item is EquipableItem equipableItem && equipableItem.Skill != null)
                    return equipableItem;
            }

            return _playerEquipment?.FindEquippedItem(itemData, equipableItem => equipableItem.Skill != null);
        }

        private int GetEquippedItemCount(ItemDataSO itemData)
        {
            return _playerEquipment?.GetEquippedItemCount(itemData) ?? 0;
        }

        private void HandleInvenSlotCount(StatSO stat, float currentValue, float prevValue)
        {
            int previousSize = (int)prevValue;
            CurrentInventorySize = (int)currentValue;

            if (CurrentInventorySize >= previousSize)
            {
                UpdateInventory();
                return;
            }

            for (int i = CurrentInventorySize; i < previousSize; i++)
            {
                ItemSlot overflowSlot = itemSlots[i];
                ItemBase overflowItem = overflowSlot.Item;
                int stack = overflowSlot.Stack;

                if (overflowItem == null)
                    continue;

                if (TryAddItem(overflowItem, stack))
                {
                    if (!ContainsItem(overflowItem))
                        overflowItem.SetOwner(null);

                    overflowSlot.Clear();
                    continue;
                }

                DropItem(overflowItem, stack, overflowSlot);
            }
        }

        public void DropItem(ItemBase overflowItem, int stack, ItemSlot overflowSlot = null)
        {
            var poolPreviewItem = _poolManagerMono.Pop<PreviewItem>(previewItem);
            Vector3 discardPos = transform.position;

            discardPos.x += Random.Range(-1f, 1f);
            discardPos.z += Random.Range(-1f, 1f);
            discardPos.y += 0.2f;

            poolPreviewItem.Discard(discardPos, overflowItem, stack);
            overflowItem.SetOwner(null);
            overflowSlot?.Clear();
            
            UpdateInventory();
        }


        protected override void OnDestroy()
        {
            InventoryChanged -= UpdateUI;
        }

        private void Start()
        {
            UpdateInventory();
        }

        private void UpdateUI()
        {
            EventBus.Raise(new UpdateLeftInventoryUIEvent
                { ItemSlots = itemSlots, SlotCnt = CurrentInventorySize });
        }
    }
}
