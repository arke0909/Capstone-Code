using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using Work.LKW.Code.ItemContainers;
using InGame.InventorySystem;
using Scripts.Combat.Datas;
using Scripts.Players;
using UnityEngine;
using Work.Code.GameEvents;
using Work.LKW.Code.Events;
using Work.LKW.Code.Items;

namespace Code.Players
{
    public class PlayerInventoryInteract : MonoBehaviour, IContainerComponent
    {
        public ComponentContainer ComponentContainer { get; set; }

        private ItemSlot _hoveringSlot;
        private ItemContainer _openedItemContainer;
        private Player _player;
        private PlayerInventory _playerInventory;
        private PlayerEquipment _playerEquipment;
        
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>();
            _player.PlayerInput.OnItemInteractPressed += HandleItemInteractPressed;
            _playerInventory = componentContainer.Get<PlayerInventory>();
            _playerEquipment = componentContainer.Get<PlayerEquipment>();
            
            EventBus.Subscribe<ItemEquipRequestEvent>(HandleItemEquipRequest);
            EventBus.Subscribe<HoveringSlotEvent>(HandleHoveringItem);
            EventBus.Subscribe<OpenItemContainerEvent>(HandleOpenItemContainer);
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUI);
        }

        private void HandleItemEquipRequest(ItemEquipRequestEvent evt)
        {
            _hoveringSlot = evt.Slot;
            HandleItemInteractPressed();
        }

        private void OnDestroy()
        {
            _player.PlayerInput.OnItemInteractPressed -= HandleItemInteractPressed;
            EventBus.Unsubscribe<ItemEquipRequestEvent>(HandleItemEquipRequest);
            EventBus.Unsubscribe<HoveringSlotEvent>(HandleHoveringItem);
            EventBus.Unsubscribe<OpenItemContainerEvent>(HandleOpenItemContainer);
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUI);
        }

        private void HandleHoveringItem(HoveringSlotEvent evt)
        {
            _hoveringSlot = evt.ItemSlot?.ItemSlot;
        }

        private void HandleItemInteractPressed()
        {
            if (_hoveringSlot == null) return;

            var item = _hoveringSlot.Item;
            if (item == null) return;

            int itemStack = _hoveringSlot.Stack;
            
            if (_hoveringSlot is EquipSlot equipSlot && item is EquipableItem equipped)
            {
                if (_playerInventory.InventoryHasBlankSlot())
                {
                    _playerEquipment.UnEquip(_playerInventory, equipSlot);
                }

                return;
            }

            // 파밍한 상자에 있다면
            if (_openedItemContainer != null && _openedItemContainer.ContainsItem(item))
            {
                _openedItemContainer.MoveItem(_playerInventory, _hoveringSlot, itemStack);
            }
            // 이미 인벤토리 안에 있다면,
            else if (_playerInventory.ContainsItem(item))
            {
                if (item is EquipableItem equipalbeItem and not UsableItem and not ThrowableItem)
                {
                    bool isSuccess = _playerEquipment.EquipByKey(equipalbeItem, _hoveringSlot);
                    if (isSuccess)
                    {
                        // 장착 성공하면 아이템 삭제
                        _playerInventory.RemoveItem(equipalbeItem, 1, false);
                    }
                        
                }
                else if (_openedItemContainer != null)
                {
                    _playerInventory.MoveItem(_openedItemContainer, _hoveringSlot, itemStack);
                }
            }
        }

        private void HandleOpenItemContainer(OpenItemContainerEvent evt)
        {
            _openedItemContainer = evt.ItemContainer;
        }

        private void HandlePlayerUI(PlayerUIEvent evt)
        {
            if (!evt.IsEnabled)
                _openedItemContainer = null;
        }
    }
}