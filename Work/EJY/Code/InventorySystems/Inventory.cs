using Chipmunk.ComponentContainers;
using System;
using System.Collections.Generic;
using System.Linq;
using Code.InventorySystems.Items;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.InventorySystems
{
    public abstract class Inventory : MonoBehaviour, IContainerComponent
    {
        public Entity Owner { get; private set; }
        public ComponentContainer ComponentContainer { get; set; }
        [SerializeField] private int _currentInventorySize = 4;

        protected int CurrentInventorySize
        {
            get => _currentInventorySize;
            set
            {
                if (value > itemSlots.Count)
                {
                    for (int i = itemSlots.Count; i < value; i++)
                    {
                        CreateSlot(i);
                    }
                }

                _currentInventorySize = value;
            }
        }

        [SerializeField] protected List<ItemSlot> itemSlots;

        public event Action InventoryChanged;

        protected virtual void Awake()
        {
            for (int i = 0; i < CurrentInventorySize; ++i)
            {
                CreateSlot(i);
            }
        }

        protected virtual void CreateSlot(int idx)
        {
            ItemSlot slot = new ItemSlot(null);
            slot.SetOwner(this);
            slot.SetIndex(idx);
            itemSlots.Add(slot);
        }

        protected virtual void OnDestroy()
        {
        }

        public ItemSlot GetItemSlot(ItemDataSO itemData)
        {
            for (int i = 0; i < CurrentInventorySize; i++)
            {
                var slot = itemSlots[i];

                if (itemData == null)
                {
                    if (slot.Item == null)
                        return slot;
                }
                else
                {
                    if (slot.Item != null && slot.Item.ItemData == itemData)
                        return slot;
                }
            }

            return null;
        }

        public IEnumerable<ItemSlot> GetItemSlots(ItemDataSO itemData)
        {
            for (int i = 0; i < CurrentInventorySize; i++)
            {
                var slot = itemSlots[i];

                if (itemData == null)
                {
                    if (slot.Item == null)
                        yield return slot;
                }
                else
                {
                    if (slot.Item != null && slot.Item.ItemData == itemData)
                        yield return slot;
                }
            }
        }

        public List<T> GetItems<T>() where T : ItemBase
        {
            List<T> items = new List<T>();
            HashSet<ItemDataSO> itemDataHashSet = new HashSet<ItemDataSO>();

            for (int i = 0; i < CurrentInventorySize; ++i)
            {
                if (itemSlots[i].Item is T typedItem && itemDataHashSet.Add(typedItem.ItemData))
                {
                    items.Add(typedItem);
                }
            }

            return items;
        }

        // 실질적으로 넣을 수 있는 아이템의 개수 구하기
        public int GetAddableItemCount(ItemBase item, int requestCount)
        {
            // 타겟 인벤토리의 같은 아이템 슬롯들의 남은 공간
            List<ItemSlot> targetSlots = GetItemSlots(item.ItemData).ToList();
            // 타겟 인벤토리의 빈 슬롯 수
            targetSlots.AddRange(GetItemSlots(null));

            int addableCnt = 0;

            foreach (var slot in targetSlots)
            {
                if (slot.Item == null)
                {
                    addableCnt += item.ItemData.maxStack;
                }
                else
                {
                    addableCnt += item.ItemData.maxStack - slot.Stack;
                }


                if (addableCnt >= requestCount)
                {
                    addableCnt = requestCount;
                    break;
                }
            }

            return addableCnt;
        }

        public bool InventoryHasBlankSlot()
        {
            for (int i = 0; i < CurrentInventorySize; ++i)
            {
                var slot = itemSlots[i];
                if (slot.Item == null) return true;
            }

            return false;
        }

        public bool TryConsume(Dictionary<ItemDataSO, int> cosumeItems)
        {
            if (!CanConsume(cosumeItems)) return false;

            foreach (var pair in cosumeItems)
            {
                RemoveItemByData(pair.Key, pair.Value);
            }

            return true;
        }

        public bool CanConsume(Dictionary<ItemDataSO, int> cosumeItems)
        {
            foreach (var pair in cosumeItems)
            {
                if (GetItemCount(pair.Key) < pair.Value)
                    return false;
            }

            return true;
        }

        public bool SubmitItem(ItemBase item, int count = 1)
        {
            if (count > GetItemCount(item.ItemData))
            {
                // 아이템 수량 딸림
                Debug.Log("아이템 수량 딸림");
                return false;
            }

            return RemoveItem(item, count);
        }

        public int GetItemCount(ItemDataSO item)
        {
            int cnt = 0;

            foreach (var itemSlot in GetItemSlots(item))
            {
                cnt += itemSlot.Stack;
            }

            return cnt;
        }

        public void UpdateInventory()
        {
            InventoryChanged?.Invoke();
        }

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            Owner = componentContainer.GetSubclassComponent<Entity>();
        }

        private bool ContainsExactItem(ItemBase item)
        {
            if (item == null)
                return false;

            for (int i = 0; i < CurrentInventorySize; i++)
            {
                if (itemSlots[i].Item == item)
                    return true;
            }

            return false;
        }

        private ItemBase CreateItemInstance(ItemBase source)
        {
            ItemCreateData createData = source.ItemData.CreateItem();
            createData.Item.SetOwner(Owner);
            return createData.Item;
        }

        private int AddItemInternal(ItemBase item, int count, bool allowReuseSourceReference = false)
        {
            int remain = count;

            foreach (var slot in GetItemSlots(item.ItemData))
            {
                if (slot.IsBlank || slot.IsFull)
                    continue;

                int addAmount = Mathf.Min(remain, item.ItemData.maxStack - slot.Stack);
                slot.AddItem(addAmount);
                remain -= addAmount;

                if (remain <= 0)
                    return count;
            }

            bool canReuseSourceReference = allowReuseSourceReference;

            foreach (var slot in GetItemSlots(null))
            {
                if (remain <= 0)
                    break;

                int addAmount = Mathf.Min(remain, item.ItemData.maxStack);
                ItemBase slotItem;

                if (canReuseSourceReference)
                {
                    slotItem = item;
                    slotItem.SetOwner(Owner);
                    canReuseSourceReference = false;
                }
                else
                {
                    slotItem = CreateItemInstance(item);
                }

                slot.SetData(slotItem, addAmount);
                remain -= addAmount;
            }

            return count - remain;
        }

        public bool TryAddItem(ItemBase item, int count = 1)
        {
            if (item == null || count <= 0)
                return false;

            if (GetAddableItemCount(item, count) < count)
                return false;

            bool allowReuseSourceReference = item.ItemData.maxStack == 1 && count == 1;
            int added = AddItemInternal(item, count, allowReuseSourceReference);

            if (added != count)
                return false;

            UpdateInventory();
            return true;
        }

        public bool RemoveItem(ItemBase item, int count = 1, bool isFirst = true)
        {
            if (item == null || count <= 0)
                return false;

            if (!isFirst)
            {
                ItemSlot targetSlot = itemSlots
                    .Take(CurrentInventorySize)
                    .FirstOrDefault(slot => slot.Item == item);

                if (targetSlot == null || targetSlot.Stack < count)
                    return false;

                bool emptied = targetSlot.Stack == count;
                ItemBase removedItem = targetSlot.Item;

                targetSlot.RemoveItem(count);

                if (emptied && !ContainsExactItem(removedItem) && (removedItem is EquipableItem equipableItem && equipableItem.IsEquipped) == false)
                    removedItem.SetOwner(null);

                UpdateInventory();
                return true;
            }

            List<ItemSlot> targetSlots = GetItemSlots(item.ItemData)
                .Where(slot => slot.Item != null)
                .ToList();

            if (targetSlots.Sum(slot => slot.Stack) < count)
                return false;

            int remaining = count;

            foreach (var slot in targetSlots)
            {
                int removeAmount = Mathf.Min(slot.Stack, remaining);
                bool emptied = slot.Stack == removeAmount;
                ItemBase removedItem = slot.Item;

                slot.RemoveItem(removeAmount);

                if (emptied && !ContainsExactItem(removedItem) && (removedItem is EquipableItem equipableItem && equipableItem.IsEquipped) == false)
                    removedItem.SetOwner(null);

                remaining -= removeAmount;

                if (remaining <= 0)
                    break;
            }

            UpdateInventory();
            return true;
        }

        public bool RemoveItemByData(ItemDataSO data, int count)
        {
            if (count <= 0)
                return false;

            List<ItemSlot> targetSlots = GetItemSlots(data)
                .Where(slot => slot.Item != null)
                .ToList();

            if (targetSlots.Sum(slot => slot.Stack) < count)
                return false;

            int remaining = count;

            foreach (var slot in targetSlots)
            {
                int removeAmount = Mathf.Min(slot.Stack, remaining);
                bool emptied = slot.Stack == removeAmount;
                ItemBase removedItem = slot.Item;

                slot.RemoveItem(removeAmount);

                if (emptied && !ContainsExactItem(removedItem))
                    removedItem.SetOwner(null);

                remaining -= removeAmount;

                if (remaining <= 0)
                    break;
            }

            UpdateInventory();
            return true;
        }

        public bool ContainsItem(ItemBase item) => ContainsExactItem(item);

        public int MoveItem(Inventory target, ItemSlot sourceSlot, int amount)
        {
            if (target == null || sourceSlot == null || sourceSlot.Item == null)
                return 0;

            amount = Mathf.Clamp(amount, 1, sourceSlot.Stack);

            ItemBase sourceItem = sourceSlot.Item;
            int addable = target.GetAddableItemCount(sourceItem, amount);
            if (addable <= 0)
                return 0;

            bool reuseSourceReference = sourceItem.ItemData.maxStack == 1 && addable == 1;
            int moved = target.AddItemInternal(sourceItem, addable, reuseSourceReference);
            if (moved <= 0)
                return 0;

            bool emptied = sourceSlot.Stack == moved;
            sourceSlot.RemoveItem(moved);

            if (emptied && !reuseSourceReference && !ContainsExactItem(sourceItem))
                sourceItem.SetOwner(null);

            target.UpdateInventory();
            UpdateInventory();
            return moved;
        }


        public void SortInventory()
        {
            var activeSlots = itemSlots.GetRange(0, CurrentInventorySize);

            activeSlots.Sort((x, y) =>
            {
                bool xBlank = x == null || x.Item == null;
                bool yBlank = y == null || y.Item == null;

                if (xBlank && yBlank)
                    return 0;
                if (xBlank)
                    return 1;
                if (yBlank)
                    return -1;

                int typeCompare = x.Item.ItemData.itemType.CompareTo(y.Item.ItemData.itemType);
                if (typeCompare != 0)
                    return typeCompare;

                int rarityCompare = x.Item.ItemData.rarity.CompareTo(y.Item.ItemData.rarity);
                if (rarityCompare != 0)
                    return rarityCompare;

                return string.Compare(
                    x.Item.ItemData.itemName,
                    y.Item.ItemData.itemName,
                    StringComparison.Ordinal
                );
            });

            for (int i = 0; i < CurrentInventorySize; i++)
            {
                itemSlots[i] = activeSlots[i];
            }

            UpdateInventory();
        }
    }
}