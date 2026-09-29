using System;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using Code.InventorySystems;
using Code.InventorySystems.Equipments;
using Scripts.Players;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;
using static Code.InventorySystems.InventoryUtility;

namespace Code.Players
{
    public class PlayerEquipment : EntityEquipment, IAfterInitialze
    {
        [SerializeField] private EquipSlotDefineListSO equipSlotDefineList;
        [SerializeField] private SoundID equipSound;
        [SerializeField] private SoundID unequipSound;

        private Player _player;
        private PlayerInventory _playerInventory;

        // 플레리어의 슬롯
        private List<EquipSlot> _equipSlots = new List<EquipSlot>();
        public IReadOnlyList<EquipSlot> EquipSlots => _equipSlots;

        public event Action OnEquipItem;
        public event Action OnUnEquipItem;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _player = componentContainer.Get<Player>(true);
            _playerInventory = componentContainer.Get<PlayerInventory>();

            EventBus.Subscribe<EquipByDragEvent>(HandleEquipByDrag);
            EventBus.Subscribe<UnEquipByDragEvent>(HandleUnEquipByDrag);
        }

        public void AfterInitialize()
        {
            for (int i = 0; i < equipSlotDefineList.equipSlotDefines.Count; ++i)
            {
                var equipSlot = new EquipSlot(null, equipSlotDefineList.equipSlotDefines[i]);
                equipSlot.SetOwner(_playerInventory);
                _equipSlots.Add(equipSlot);
            }
        }

        private void Start()
        {
            EventBus<UpdateEquipUIEvent>.Raise(new UpdateEquipUIEvent(_equipSlots));
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<EquipByDragEvent>(HandleEquipByDrag);
            EventBus.Unsubscribe<UnEquipByDragEvent>(HandleUnEquipByDrag);
        }

        private void HandleEquipByDrag(EquipByDragEvent evt)
        {
            EquipFromSlot(evt.StartSlot, _equipSlots[evt.Index]);
        }

        private void HandleUnEquipByDrag(UnEquipByDragEvent evt)
        {
            UnEquipToSlot(evt.EquipSlot, evt.TargetSlot);
        }

        // 키를 통한 장착
        public bool EquipFromInventory(EquipableItem equipableItem, ItemSlot sourceSlot)
        {
            EquipSlotType slotType = equipableItem.EquipItemData.itemType.GetEquipSlotType();
            if (slotType == EquipSlotType.None) return false;

            // 슬롯 타입이 같고 비어있는 장비칸 탐색
            var equipSlot = _equipSlots.FirstOrDefault(slot => slot.EquipSlotType == slotType && slot.IsBlank);

            // 없다면 가장 마지막 무기 슬롯에 교체
            if (equipSlot == null)
                equipSlot = _equipSlots.LastOrDefault(slot => slot.EquipSlotType == slotType);

            // 그래도 없으면 잘못된 타입
            if (equipSlot == null) return false;

            if (!EquipToSlot(equipSlot, equipableItem, sourceSlot))
                return false;

            // 교체가 아니면 원본 슬롯에 아직 새 장비가 남아 있으니 비운다.
            if (sourceSlot.Item == equipableItem)
                sourceSlot.SetData(null);

            sourceSlot.OwnerInventory?.UpdateInventory();
            return true;
        }

        public bool EquipFromSlot(ItemSlot slot, EquipSlot equipSlot)
        {
            EquipableItem equipableItem = slot?.Item as EquipableItem;

            if (slot == null || equipableItem == null) return false;

            if (!EquipToSlot(equipSlot, equipableItem, slot))
                return false;

            if (slot.Item == equipableItem)
                slot.SetData(null);

            return true;
        }

        private bool EquipToSlot(EquipSlot equipSlot, EquipableItem equipableItem, ItemSlot sourceSlot)
        {
            if (equipSlot == null || equipableItem == null) return false;

            // 이미 장착된게 있는지 확인, 없으면 추가 있으면 교체
            if (equipSlot.Item != null)
            {
                if (!UnequipFromSlot(equipSlot, out EquipableItem equipped))
                    return false;

                if (!TryStoreUnequippedItem(equipped, sourceSlot, true))
                    _playerInventory.DropItem(equipped, 1);
            }

            equipSlot.SetData(equipableItem, 1);
            RegisterSkill(equipSlot, equipableItem);

            EquipPartType equipPartType = equipSlot.EquipPartType;

            int equipSlotLocalIndex = GetLocalIndex(equipSlot.Index);

            if (equipSlot.CanHandle)
                EventBus.Raise(new EquipHotbarEvent(equipSlotLocalIndex, equipableItem));

            if (!equipSlot.CanHandle && !EquipItemToPart(equipPartType, equipableItem))
            {
                equipSlot.SetData(null);
                DeregisterSkill(equipSlot, equipableItem);
                return false;
            }

            _player.LocalEventBus.Raise(new EquipItemEvent(equipSlot));
            EventBus.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));
            OnEquipItem?.Invoke();
            BroAudio.Play(equipSound);

            return true;
        }

        public bool UnEquipToInventory(EquipSlot equipSlot)
        {
            if (!UnequipFromSlot(equipSlot, out EquipableItem equipped))
                return false;

            if (TryStoreUnequippedItem(equipped))
                return true;

            _playerInventory.DropItem(equipped, 1);
            return true;
        }

        public bool UnEquipToSlot(EquipSlot equipSlot, ItemSlot targetSlot)
        {
            if (targetSlot == null || !targetSlot.IsBlank)
                return false;

            if (!UnequipFromSlot(equipSlot, out EquipableItem equipped))
                return false;

            if (TryStoreUnequippedItem(equipped, targetSlot))
                return true;

            _playerInventory.DropItem(equipped, 1);
            return true;
        }

        public bool DropEquippedItem(EquipableItem item)
        {
            if (item == null)
                return false;

            EquipSlot equipSlot = _equipSlots.FirstOrDefault(slot => slot.Equipable == item);
            if (equipSlot == null)
                return false;

            int stack = equipSlot.Stack;

            if (!UnequipFromSlot(equipSlot, out EquipableItem equipped))
                return false;

            _playerInventory.DropItem(equipped, stack);
            return true;
        }

        public int GetEquippedItemCount(ItemDataSO itemData)
        {
            if (itemData == null)
                return 0;

            int count = 0;

            foreach (EquipSlot equipSlot in _equipSlots)
            {
                if (equipSlot.Item?.ItemData == itemData)
                    count += equipSlot.Stack;
            }

            return count;
        }

        public bool TryConsumeEquippedItems(ItemDataSO itemData, int count)
        {
            if (itemData == null || count <= 0)
                return false;

            if (GetEquippedItemCount(itemData) < count)
                return false;

            int remaining = count;
            bool consumed = false;

            foreach (EquipSlot equipSlot in _equipSlots.ToList())
            {
                if (remaining <= 0)
                    break;

                if (equipSlot.Item?.ItemData != itemData)
                    continue;

                int equippedStack = equipSlot.Stack;
                if (!UnequipFromSlot(equipSlot, out EquipableItem equipped))
                    return false;

                equipped.SetOwner(null);
                remaining -= equippedStack;
                consumed = true;
            }

            if (remaining > 0)
                return false;

            if (consumed)
                _playerInventory.UpdateInventory();

            return true;
        }

        public EquipableItem FindEquippedItem(ItemDataSO itemData, Predicate<EquipableItem> match = null)
        {
            foreach (EquipSlot equipSlot in _equipSlots)
            {
                if (equipSlot.Item is not EquipableItem equipableItem)
                    continue;

                if (itemData != null && equipableItem.ItemData != itemData)
                    continue;

                if (match != null && !match(equipableItem))
                    continue;

                return equipableItem;
            }

            return null;
        }

        public EquipSlot FindEquippedSlot(ItemDataSO itemData)
        {
            if (itemData == null)
                return null;

            return _equipSlots.FirstOrDefault(slot => slot.Item?.ItemData == itemData);
        }

        public bool EquipCraftedItemToSlot(EquipSlot equipSlot, EquipableItem equipableItem)
        {
            if (equipSlot == null || equipableItem == null || !equipSlot.IsBlank || !equipSlot.CanEquip(equipableItem))
                return false;

            return EquipToSlot(equipSlot, equipableItem, null);
        }

        private bool TryStoreUnequippedItem(EquipableItem equipped, ItemSlot preferredSlot = null,
            bool allowOverwritePreferredSlot = false)
        {
            if (equipped == null)
                return false;

            Inventory preferredInventory = preferredSlot?.OwnerInventory;
            if (preferredInventory != null &&
                preferredInventory.IsActiveSlot(preferredSlot) &&
                (preferredSlot.IsBlank || allowOverwritePreferredSlot))
            {
                preferredSlot.SetData(equipped, 1);
                preferredInventory.UpdateInventory();
                return true;
            }

            return _playerInventory.TryAddItem(equipped);
        }

        private bool UnequipFromSlot(EquipSlot equipSlot, out EquipableItem equipped)
        {
            equipped = equipSlot?.Equipable;
            if (equipSlot == null || equipped == null)
                return false;

            EquipPartType equipPartType = equipSlot.EquipPartType;

            equipSlot.SetData(null);

            DeregisterSkill(equipSlot, equipped);

            if (equipSlot.CanHandle)
                EventBus.Raise(new UnEquipHotbarEvent(GetLocalIndex(equipSlot.Index)));


            _player.LocalEventBus.Raise(new UnequipItemEvent(equipSlot, equipped));

            if (!equipSlot.CanHandle && GetEquippedItem(equipPartType) == equipped)
                UnequipItemFromPart(equipPartType, out _);

            EventBus.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));
            OnUnEquipItem?.Invoke();
            BroAudio.Play(unequipSound);

            return true;
        }

        public void RegisterSkill(EquipSlot equipSlot, EquipableItem equipableItem)
        {
            if (equipSlot != null && equipSlot.HasSkill && equipableItem != null)
                equipableItem.RegisterSkill();
        }

        public void DeregisterSkill(EquipSlot equipSlot, EquipableItem equipableItem)
        {
            if (equipSlot != null && equipSlot.HasSkill && equipableItem != null)
                equipableItem.DeregisterSkill();
        }

        public bool IsEquipped(EquipPartType partType)
        {
            return GetEquippedItem(partType)?.IsEquipped ?? false;
        }

        public void SetEquippedItem(EquipPartType partType, EquipableItem item)
            => EquipItemToPart(partType, item);
    }
}
