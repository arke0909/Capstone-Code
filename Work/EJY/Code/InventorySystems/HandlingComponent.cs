using System.Linq;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Code.Items;
using Code.Players;
using InGame.InventorySystem;
using Scripts.Players;
using UnityEngine;
using static Code.InventorySystems.InventoryUtility;

namespace Code.InventorySystems
{
    public class HandlingComponent : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EquipItemEvent>,
        ILocalEventSubscriber<UnequipItemEvent>
    {
        private Player _player;
        private PlayerEquipment _playerEquipment;
        private HandItem _handItem;

        private int _handlingIndex = -1;
        private int _handledIndex = -1;

        public int HandlingIndex => _handlingIndex;
        public int HandledIndex => _handledIndex;
        public HandItem CurrentHandItem => _handItem;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>(true);
            _playerEquipment = componentContainer.Get<PlayerEquipment>();
            EventBus.Subscribe<SwapEquipEvent>(HandleSwapEquip);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SwapEquipEvent>(HandleSwapEquip);
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

        private void HandleSwapEquip(SwapEquipEvent evt)
        {
            EquipSlot startEquipSlot = evt.StartEquip;
            EquipSlot targetEquipSlot = evt.TargetEquip;
            EquipableItem startSlotItem = startEquipSlot.Item as EquipableItem;
            EquipableItem targetSlotItem = targetEquipSlot.Item as EquipableItem;

            int startEquipLocalIndex = GetLocalIndex(startEquipSlot.Index);
            int targetEquipLocalIndex = GetLocalIndex(targetEquipSlot.Index);

            _playerEquipment.DeregisterSkill(startEquipSlot, startSlotItem);
            _playerEquipment.DeregisterSkill(targetEquipSlot, targetSlotItem);

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

            _playerEquipment.RegisterSkill(targetEquipSlot, startSlotItem);
            _playerEquipment.RegisterSkill(startEquipSlot, targetSlotItem);


            bool touchesCurrentHandle =
                startEquipSlot.CanHandle &&
                targetEquipSlot.CanHandle &&
                (startEquipLocalIndex == HandlingIndex || targetEquipLocalIndex == HandlingIndex);

            if (touchesCurrentHandle)
            {
                if (startSlotItem != null && targetSlotItem != null)
                {
                    if (startEquipLocalIndex == HandlingIndex)
                    {
                        SetHandItem(targetSlotItem);
                    }
                    else
                    {
                        SetHandItem(startSlotItem);
                    }
                }
                else
                {
                    EquipSlot activeSlot = startEquipLocalIndex == HandlingIndex ? startEquipSlot : targetEquipSlot;
                    EquipSlot swappedSlot = activeSlot == startEquipSlot ? targetEquipSlot : startEquipSlot;

                    if (!activeSlot.IsBlank)
                    {
                        SetHandItem(activeSlot.Equipable);
                    }
                    else if (!swappedSlot.IsBlank)
                    {
                        HandleSlotItem(swappedSlot);
                    }
                    else
                    {
                        UnHandleItem();
                    }
                }
            }

            EventBus.Raise(new UpdateEquipUIEvent(_playerEquipment.EquipSlots.ToList()));
        }

        public void ChangeHandlingHotbarItem(HandItem handItem)
        {
            if (handItem == null)
                return;

            // 이 무기가 equip slot에 실제로 꽂혀 있는 장비면 그 슬롯 index를 추적
            EquipSlot equipSlot = _playerEquipment.EquipSlots.FirstOrDefault(slot => slot.Equipable == handItem);

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

        private void SetHandItem(EquipableItem item)
        {
            HandItem handItem = item as HandItem;
            
            if (handItem == _handItem)
            {
                UnHandleItem();
                return;
            }

            _handItem?.UnHandle(_player);
            _handItem = handItem;
            _handItem?.Handle(_player, _playerEquipment.GetEquipTransform(EquipPartType.Hand));
            _playerEquipment.SetEquippedItem(EquipPartType.Hand, item);
            _player.LocalEventBus.Raise(new ChangeHandlingEvent(item));
        }

        private void UpdateHandleIndex(int idx)
        {
            _handledIndex = _handlingIndex;
            _handlingIndex = idx;
        }

        private void HandleSlotItem(EquipSlot equipSlot)
        {
            SetHandItem(equipSlot.Equipable);
            UpdateHandleIndex(GetLocalIndex(equipSlot.Index));
        }

        private void UnHandleItem()
        {
            SetHandItem(null);
            UpdateHandleIndex(-1);
        }

        public void RestoreHandledEquip()
        {
            if (_handledIndex < 0)
            {
                UnHandleItem();
                return;
            }
        
            EquipSlot handledSlot = _playerEquipment.EquipSlots
                .FirstOrDefault(slot => slot.CanHandle && GetLocalIndex(slot.Index) == _handledIndex);
        
            if (handledSlot?.Equipable != null)
            {
                HandleSlotItem(handledSlot);
                return;
            }
        
            UnHandleItem();
        }


        public void OnLocalEvent(EquipItemEvent eventData)
        {
            EquipSlot equipSlot = eventData.EquipSlot;

            if (equipSlot.Equipable != null && equipSlot.EquipPartType == EquipPartType.Hand
                                            && _handItem == null)
            {
                HandleSlotItem(equipSlot);
            }
        }

        public void OnLocalEvent(UnequipItemEvent eventData)
        {
            if (eventData.EquipSlot != null && eventData.EquipSlot.EquipPartType == EquipPartType.Hand
                                            && eventData.EquippedItem == _handItem)
            {
                if (_playerEquipment.TryChangeSpareWeapon(out EquipSlot spareSlot))
                {
                    HandleSlotItem(spareSlot);
                    return;
                }

                UnHandleItem();
            }
        }
    }
}