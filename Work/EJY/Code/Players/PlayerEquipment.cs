using System;
using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using System.Collections.Generic;
using System.Linq;
using Code.InventorySystems;
using Code.InventorySystems.Equipments;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.Items;
using static Code.InventorySystems.InventoryUtility;

namespace Code.Players
{
    public class PlayerEquipment : MonoBehaviour, IContainerComponent, IAfterInitialze
    {
        [SerializeField] private SerializedDictionary<EquipPartType, Transform> equipTrms;
        [SerializeField] private EquipSlotDefineListSO equipSlotDefineList;
        public ComponentContainer ComponentContainer { get; set; }

        private Player _player;
        private PlayerInventory _playerInventory;

        // 현재 어떤 부위에 어떤 장비를 장착하고 있는지
        private Dictionary<EquipPartType, EquipableItem> _equips = new Dictionary<EquipPartType, EquipableItem>();

        // 플레리어의 슬롯
        private List<EquipSlot> _equipSlots = new List<EquipSlot>();
        private int _handlingIndex;
        private int _handledIndex;

        public int HandlingIndex => _handlingIndex;
        public int HandledIndex => _handledIndex;

        public event Action OnEquipItem;
        public event Action OnUnEquipItem;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.GetCompo<Player>(true);
            _playerInventory = componentContainer.GetComponent<PlayerInventory>();

            EventBus.Subscribe<SwapEquipEvent>(HandleSwapEquip);
            EventBus.Subscribe<EquipByDragEvent>(HandleEquipByDrag);
            EventBus.Subscribe<UnEquipByDragEvent>(HandleUnEquipByDrag);
        }

        public void AfterInitialize()
        {
            for (int i = 0; i < (int)EquipPartType.Count; ++i)
            {
                _equips.Add((EquipPartType)i, null);
            }

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
            EventBus.Unsubscribe<SwapEquipEvent>(HandleSwapEquip);
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

        private void HandleSwapEquip(SwapEquipEvent evt)
        {
            EquipSlot startEquipSlot = evt.StartEquip;
            EquipSlot targetEquipSlot = evt.TargetEquip;
            EquipableItem startSlotItem = startEquipSlot.Item as EquipableItem;
            EquipableItem targetSlotItem = targetEquipSlot.Item as EquipableItem;

            int startEquipLocalIndex = GetLocalIndex(startEquipSlot.Index);
            int targetEquipLocalIndex = GetLocalIndex(targetEquipSlot.Index);

            if (startSlotItem == null)
            {
                targetEquipSlot.SetData(null);

                if (targetEquipSlot.CanHandle)
                    UpdateHotbarSlot(targetEquipLocalIndex);
            }
            else
            {
                targetEquipSlot.SetData(startSlotItem, 1);

                if (targetEquipSlot.CanHandle)
                    UpdateHotbarSlot(targetEquipLocalIndex, startSlotItem);
            }


            if (targetSlotItem == null)
            {
                startEquipSlot.SetData(null);

                if (startEquipSlot.CanHandle)
                    UpdateHotbarSlot(startEquipLocalIndex);
            }
            else
            {
                startEquipSlot.SetData(targetSlotItem, 1);

                if (startEquipSlot.CanHandle)
                    UpdateHotbarSlot(startEquipLocalIndex, targetSlotItem);
            }

            bool touchesCurrentHandle =
                startEquipSlot.CanHandle &&
                targetEquipSlot.CanHandle &&
                (startEquipLocalIndex == _handlingIndex || targetEquipLocalIndex == _handlingIndex);

            if (touchesCurrentHandle)
            {
                if (startSlotItem != null && targetSlotItem != null)
                {
                    if (startEquipLocalIndex == _handlingIndex)
                    {
                        RefreshHandItem(targetSlotItem);
                    }
                    else
                    {
                        RefreshHandItem(startSlotItem);
                    }
                }
                else
                {
                    EquipSlot activeSlot = startEquipLocalIndex == _handlingIndex ? startEquipSlot : targetEquipSlot;
                    EquipSlot swappedSlot = activeSlot == startEquipSlot ? targetEquipSlot : startEquipSlot;

                    if (!activeSlot.IsBlank)
                    {
                        RefreshHandItem(activeSlot.Equipable);
                    }
                    else if (!swappedSlot.IsBlank)
                    {
                        UpdateHandleIndex(GetLocalIndex(swappedSlot.Index));
                        RefreshHandItem(swappedSlot.Equipable);
                    }
                    else
                    {
                        UpdateHandleIndex(-1);
                        RefreshHandItem(null);
                    }
                }
            }

            EventBus<UpdateEquipUIEvent>.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));
        }

        private void UpdateHotbarSlot(int index, EquipableItem item = null)
        {
            if (item == null)
            {
                EventBus.Raise(new UnEquipHotbarEvent(index));
                return;
            }

            EventBus.Raise(new EquipHotbarEvent(index, item));
        }

        #region Change Handle Item About Hotbar

        public void ChangeHandlingHotbarItem(HandItem handItem)
        {
            if (handItem == null)
                return;

            // 이 무기가 equip slot에 실제로 꽂혀 있는 장비면 그 슬롯 index를 추적
            EquipSlot equipSlot = _equipSlots.FirstOrDefault(slot => slot.Equipable == handItem);

            if (equipSlot != null)
            {
                UpdateHandleIndex(GetLocalIndex(equipSlot.Index));
            }
            else
            {
                // 핫바 전용 임시 아이템이면 현재 들고 있던 equip slot만 기억
                _handledIndex = _handlingIndex;
            }

            SetHandItem(handItem);
        }

        public void RestoreHandledEquip()
        {
            EquipSlot handledSlot = _equipSlots.FirstOrDefault(slot => GetLocalIndex(slot.Index) == _handledIndex);

            if (handledSlot == null || handledSlot.Equipable == null)
            {
                ChangeSpareWeapon();
                return;
            }

            UpdateHandleIndex(GetLocalIndex(handledSlot.Index));
            SetHandItem(handledSlot.Equipable);
        }

        private void SetHandItem(EquipableItem item)
        {
            if (_equips[EquipPartType.Hand] != null)
                _equips[EquipPartType.Hand].Unequip(_player);

            _equips[EquipPartType.Hand] = item;

            if (item != null)
                item.Equip(_player, equipTrms[EquipPartType.Hand]);

            EventBus.Raise(new ChangeHandlingEvent(item));
        }

        #endregion

        public void RefreshHandItem(EquipableItem newItem)
        {
            if (_equips[EquipPartType.Hand] != null)
                _equips[EquipPartType.Hand].Unequip(_player);

            _equips[EquipPartType.Hand] = newItem;

            if (newItem != null)
            {
                newItem.Equip(_player, equipTrms[EquipPartType.Hand]);
            }

            EventBus.Raise(new ChangeHandlingEvent(newItem));
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

            if (!Equip(equipSlot, equipableItem, sourceSlot))
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

            if (!Equip(equipSlot, equipableItem, slot))
                return false;
            
            if(slot.Item == equipableItem)
                slot.SetData(null);
            
            return true;
        }

        private bool Equip(EquipSlot equipSlot, EquipableItem equipableItem, ItemSlot sourceSlot)
        {
            if (equipSlot == null || equipableItem == null) return false;

            // 이미 장착된게 있는지 확인, 없으면 추가 있으면 교체
            if (equipSlot.Item != null)
            {
                if (!UnEquip(equipSlot, out EquipableItem equipped))
                    return false;
                
                sourceSlot.SetData(equipped, 1);
            }

            equipSlot.SetData(equipableItem, 1);
            if (equipSlot.HasSkill)
                equipableItem.RegisterSkill();

            EquipPartType equipPartType = equipSlot.EquipPartType;

            int equipSlotLocalIndex = GetLocalIndex(equipSlot.Index);

            if (_equips.TryGetValue(equipPartType, out EquipableItem equippingItem) && equippingItem == null)
            {
                _equips[equipPartType] = equipableItem;
                equipableItem.Equip(_player, equipTrms[equipPartType]);
                
                if (equipPartType == EquipPartType.Hand)
                {
                    UpdateHandleIndex(equipSlotLocalIndex);
                    EventBus.Raise(new ChangeHandlingEvent(equipableItem));
                }
            }

            if (equipPartType == EquipPartType.Hand)
                EventBus.Raise(new EquipHotbarEvent(equipSlotLocalIndex, equipableItem));

            EventBus.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));
            OnEquipItem?.Invoke();

            return true;
        }

        public bool UnEquipToInventory(EquipSlot equipSlot)
        {
            if (!UnEquip(equipSlot, out EquipableItem equipped))
                return false;

            return _playerInventory.TryAddItem(equipped);
        }

        public bool UnEquipToSlot(EquipSlot equipSlot, ItemSlot targetSlot)
        {
            if (targetSlot == null || !targetSlot.IsBlank)
                return false;

            if (!UnEquip(equipSlot, out EquipableItem equipped))
                return false;

            targetSlot.SetData(equipped, 1);
            return true;
        }

        private bool UnEquip(EquipSlot equipSlot, out EquipableItem equipped)
        {
            equipped = equipSlot?.Equipable;
            if (equipSlot == null || equipped == null)
                return false;

            EquipPartType equipPartType = equipSlot.EquipPartType;

            if (!_equips.ContainsKey(equipPartType))
                return false;

            if (equipPartType == EquipPartType.Hand)
                EventBus.Raise(new UnEquipHotbarEvent(GetLocalIndex(equipSlot.Index)));

            equipSlot.SetData(null);

            if (equipSlot.HasSkill)
                equipped.DeregisterSkill();

            if (equipped.IsEquipped)
            {
                equipped.Unequip(_player);
                _equips[equipPartType] = null;

                if (equipPartType == EquipPartType.Hand)
                    ChangeSpareWeapon();
            }

            EventBus.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));
            OnUnEquipItem?.Invoke();
            
            return true;
        }


        private void ChangeSpareWeapon()
        {
            var equipSlot = _equipSlots.FirstOrDefault(slot => slot.CanHandle && !slot.IsBlank);

            if (equipSlot == null)
            {
                SetHandItem(null);
                UpdateHandleIndex(-1);
                return;
            }

            var spareWeapon = equipSlot.Equipable;
            SetHandItem(spareWeapon);
            UpdateHandleIndex(GetLocalIndex(equipSlot.Index));
        }

        private void UpdateHandleIndex(int idx)
        {
            _handledIndex = _handlingIndex;
            _handlingIndex = idx;
        }

        public bool TryGetEquippedItem(EquipPartType partType, out EquipableItem item)
        {
            item = GetEquippedItem(partType);
            if (item == null)
                return false;
            return true;
        }

        public EquipableItem GetEquippedItem(EquipPartType partType) => _equips.GetValueOrDefault(partType);
    }
}