using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using System.Collections.Generic;
using System.Linq;
using Code.InventorySystems.Equipments;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Code.Players
{
    public class PlayerEquipment : MonoBehaviour, IContainerComponent
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

        public void OnInitialize(ComponentContainer componentContainer)
        {
            for (int i = 0; i < (int)EquipPartType.Count; ++i)
            {
                _equips.Add((EquipPartType)i, null);
            }

            for (int i = 0; i < equipSlotDefineList.equipSlotDefines.Count; ++i)
            {
                var equipSlot = new EquipSlot(null, equipSlotDefineList.equipSlotDefines[i]);
                _equipSlots.Add(equipSlot);
            }

            _player = componentContainer.GetCompo<Player>(true);
            _playerInventory = componentContainer.GetComponent<PlayerInventory>();

            EventBus.Subscribe<SwapEquipEvent>(HandleSwapEquip);
            EventBus.Subscribe<EquipByDragEvent>(HandleEquipByDrag);
            EventBus.Subscribe<UnEquipByDragEvent>(HandleUnEquipByDrag);
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
            if (evt.Item is EquipableItem equipalbeItem)
            {
                bool isSuccess = Equip(equipalbeItem, _equipSlots[evt.Index],evt.StartSlot, true);
                
                if (isSuccess)
                {
                    evt.OnSuccessCallback?.Invoke();
                }
            }
        }

        private void HandleUnEquipByDrag(UnEquipByDragEvent evt)
        {
            if (evt.Item is EquipableItem equipalbeItem)
            {
                if (UnEquip(equipalbeItem, evt.EquipSlot, byDrag: true))
                    evt.TargetSlot.SetData(equipalbeItem, 1);
            }
        }

        private void HandleSwapEquip(SwapEquipEvent evt)
        {
            EquipSlot startEquipSlot = evt.StartEquip;
            EquipSlot targetEquipSlot = evt.TargetEquip;
            EquipableItem startSlotItem = startEquipSlot.Item as EquipableItem;
            EquipableItem targetSlotItem = targetEquipSlot.Item as EquipableItem;

            if (startSlotItem == null)
            {
                targetEquipSlot.SetData(null);

                if (targetEquipSlot.CanHandle)
                    UpdateHotbarSlot(targetEquipSlot.Index);
            }
            else
            {
                targetEquipSlot.SetData(startSlotItem, 1);

                if (targetEquipSlot.CanHandle)
                    UpdateHotbarSlot(targetEquipSlot.Index, startSlotItem);
            }

            if (targetSlotItem == null)
            {
                startEquipSlot.SetData(null);

                if (startEquipSlot.CanHandle)
                    UpdateHotbarSlot(startEquipSlot.Index);
            }
            else
            {
                startEquipSlot.SetData(targetSlotItem, 1);

                if (startEquipSlot.CanHandle)
                    UpdateHotbarSlot(startEquipSlot.Index, targetSlotItem);
            }

            bool touchesCurrentHandle =
                startEquipSlot.CanHandle &&
                targetEquipSlot.CanHandle &&
                (startEquipSlot.Index == _handlingIndex || targetEquipSlot.Index == _handlingIndex);

            if (touchesCurrentHandle)
            {
                if (startSlotItem != null && targetSlotItem != null)
                {
                    if (startEquipSlot.Index == _handlingIndex)
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
                    EquipSlot activeSlot = startEquipSlot.Index == _handlingIndex ? startEquipSlot : targetEquipSlot;
                    EquipSlot swappedSlot = activeSlot == startEquipSlot ? targetEquipSlot : startEquipSlot;

                    if (!activeSlot.IsBlank)
                    {
                        RefreshHandItem(activeSlot.Equipable);
                    }
                    else if (!swappedSlot.IsBlank)
                    {
                        UpdateHandleIndex(swappedSlot.Index);
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
        public void ChangeHandlingHotbarItem(Weapon weapon)
        {
            if (weapon == null)
                return;

            // 이 무기가 equip slot에 실제로 꽂혀 있는 장비면 그 슬롯 index를 추적
            EquipSlot equipSlot = _equipSlots.FirstOrDefault(slot => slot.Equipable == weapon);

            if (equipSlot != null)
            {
                UpdateHandleIndex(equipSlot.Index);
            }
            else
            {
                // 핫바 전용 임시 아이템이면 현재 들고 있던 equip slot만 기억
                _handledIndex = _handlingIndex;
            }

            SetHandItem(weapon);
        }

        public void RestoreHandledEquip()
        {
            _equips[EquipPartType.Hand]?.Unequip(_player);
            
            if (_handledIndex < 0)
                return;

            EquipSlot handledSlot = _equipSlots.FirstOrDefault(slot => slot.Index == _handledIndex);

            if (handledSlot == null || handledSlot.Equipable == null)
            {
                ChangeSpareWeapon();
                return;
            }

            _handlingIndex = handledSlot.Index;
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
        public bool EquipByKey(EquipableItem equipableItem, ItemSlot sourceSlot)
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

            return Equip(equipableItem, equipSlot, sourceSlot,false);
        }

        private bool Equip(EquipableItem equipable, EquipSlot equipSlot, ItemSlot sourceSlot, bool byDrag)
        {
            if (equipSlot == null || equipable == null) return false;


            // 이미 장착된게 있는지 확인, 없으면 추가 있으면 교체
            if (equipSlot.Item != null)
            {
                EquipableItem equipped = equipSlot.Item as EquipableItem;

                if (!UnEquip(equipped, equipSlot, sourceSlot, byDrag)) return false;
            }

            equipSlot.SetData(equipable, 1);

            EquipPartType equipPartType = equipSlot.EquipPartType;

            if (_equips.TryGetValue(equipPartType, out EquipableItem equippingItem) && equippingItem == null)
            {
                _equips[equipPartType] = equipable;
                if (equipPartType == EquipPartType.Hand)
                {
                    UpdateHandleIndex(equipSlot.Index);
                    EventBus.Raise(new ChangeHandlingEvent(equipable));
                }

                equipable.Equip(_player, equipTrms[equipPartType]);
            }

            if (equipPartType == EquipPartType.Hand)
                EventBus.Raise(new EquipHotbarEvent(equipSlot.Index, equipable));

            EventBus.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));

            return true;
        }
        
        public bool UnEquip(EquipableItem equipped, EquipSlot equipSlot, ItemSlot sourceSlot = null , bool byDrag = false)
        {
            bool isExchange = sourceSlot != null;

            if (equipSlot == null || equipped == null)
                return false;

            EquipPartType equipPartType = equipSlot.EquipPartType;

            if (!_equips.ContainsKey(equipPartType))
                return false;

            // 일반 해제일 때만 인벤토리 빈칸 체크
            if (!isExchange && !_playerInventory.InventoryHasBlankSlot())
                return false;

            if (equipPartType == EquipPartType.Hand)
            {
                EventBus.Raise(new UnEquipHotbarEvent(equipSlot.Index));
            }

            // 장비 슬롯 비우기
            equipSlot.SetData(null);

            // 실제 장착 중이던 아이템이면 외형/스탯 해제
            if (equipped.IsEquipped)
            {
                equipped.Unequip(_player);
                _equips[equipPartType] = null;

                // 일반 해제일 때만 예비 무기 자동 장착
                if (!isExchange && equipPartType == EquipPartType.Hand)
                    ChangeSpareWeapon();
            }

            if (isExchange)
            {
                // 교체면 기존 장비를 source slot으로 돌려줌
                sourceSlot.SetData(equipped, 1);
            }
            else
            {
                if (!byDrag)
                    _playerInventory.TryAddItem(equipped);
            }

            EventBus.Raise(new UpdateEquipUIEvent(_equipSlots.ToList()));
            return true;
        }

        private void ChangeSpareWeapon()
        {
            var equipSlot = _equipSlots.FirstOrDefault(slot => slot.CanHandle && !slot.IsBlank);

            if (equipSlot == null)
            {
                UpdateHandleIndex(-1);
                EventBus.Raise(new ChangeHandlingEvent(null));
                return;
            }
            
            var spareWeapon = equipSlot.Equipable;
            _equips[EquipPartType.Hand] = spareWeapon;
            _equips[EquipPartType.Hand].Equip(_player, equipTrms[EquipPartType.Hand]);
            UpdateHandleIndex(equipSlot.Index);
            EventBus.Raise(new ChangeHandlingEvent(spareWeapon));
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
