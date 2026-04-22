using Chipmunk.ComponentContainers;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using System;
using System.Collections.Generic;
using System.Linq;
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
        protected int CurrentInventorySize { get => _currentInventorySize;set 
            {
                if (value > itemSlots.Count)
                {
                    int loop = value - itemSlots.Count;
                    for (int i = 0; i < loop; i++)
                    {
                        CreateSlot();
                    }
                }
                _currentInventorySize = value;
            } }

        [SerializeField] protected List<ItemSlot> itemSlots;

        public event Action InventoryChanged;

        protected virtual void Awake()
        {
            for (int i = 0; i < CurrentInventorySize; ++i)
            {
                CreateSlot();
            }
        }

        private void CreateSlot()
        {
            ItemSlot slot = new ItemSlot(null);
            slot.SetOwner(this);
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
        
        private int AddItemInternal(ItemBase item, int count)
        {
            int remain = count;

            var slots = GetItemSlots(item.ItemData);

            foreach (var slot in slots)
            {
                if (slot.IsFull)
                    continue;

                remain = slot.AddItem(remain);
                
                if(remain <= 0)
                    return count;
            }
            
            slots = GetItemSlots(null);

            foreach (var slot in slots)
            {
                int addAmount = Mathf.Min(remain, item.ItemData.maxStack);
                item.SetOwner(Owner);
                slot.SetData(item, addAmount);
                remain -= addAmount;

                if (remain <= 0)
                    return count;
            }
            
            return count - remain;
        }

        public bool TryAddItem(ItemBase item, int count = 1)
        {
            if (item == null || count <= 0)
                return false;

            if (GetAddableItemCount(item, count) < count)
                return false;

            int added = AddItemInternal(item, count);

            if (added != count)
                return false;

            UpdateInventory();
            return true;
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

        public bool RemoveItem(ItemBase item, int count = 1, bool isFirst = true)
        {
            if (item == null || count <= 0)
                return false;

            int remaining = count;

            var slots = GetItemSlots(item.ItemData);

            foreach (var slot in slots)
            {
                if (!isFirst && slot.Item != item)
                    continue;
                
                remaining = slot.RemoveItem(remaining);

                if (remaining <= 0)
                {
                    item.SetOwner(null);
                    UpdateInventory();
                    return true;
                }

                if (!isFirst)
                    break;
            }
            
            return false;
        }

        public bool InventoryHasBlankSlot()
        {
            for (int i = 0; i < CurrentInventorySize; ++i)
            {
                var slot = itemSlots[i];
                if(slot.Item == null) return true;
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

        public bool RemoveItemByData(ItemDataSO data, int count)
        {
            if (count <= 0) return false;

            int totalCount = GetItemCount(data);
            if (totalCount < count) return false;

            int remaining = count;

            foreach (var slot in GetItemSlots(data))
            {
                if (slot.Item == null) continue;

                int removeAmount = Mathf.Min(slot.Stack, remaining);
                slot.RemoveItem(removeAmount);
                remaining -= removeAmount;

                if (remaining <= 0)
                    break;
            }

            UpdateInventory();
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

        public bool ContainsItem(ItemBase item) => itemSlots.FirstOrDefault(slot => slot.Item == item) != default;

        public int MoveItem(Inventory target, ItemSlot sourceSlot, int amount)
        {
            if (sourceSlot.Item == null)
                return 0;

            amount = Mathf.Clamp(amount, 1, sourceSlot.Stack);

            int addable = target.GetAddableItemCount(sourceSlot.Item, amount);
            if (addable <= 0)
                return 0;

            int moved = target.AddItemInternal(sourceSlot.Item, addable);

            if (moved > 0)
            {
                sourceSlot.RemoveItem(moved);
                target.UpdateInventory();
                UpdateInventory();
            }

            return moved;
        }
        
        public void UpdateInventory()
        {
            InventoryChanged?.Invoke();
        }

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
            Owner = componentContainer.GetSubclassComponent<Entity>();
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
