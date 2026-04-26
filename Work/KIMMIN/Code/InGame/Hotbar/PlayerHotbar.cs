using AYellowpaper.SerializedCollections;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Hotbar;
using Code.InventorySystems.Items;
using Code.Players;
using Scripts.Combat.Datas;
using Scripts.Players;
using Scripts.Players.States;
using System.Collections.Generic;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Code.InventorySystem
{
    public enum HotbarType
    {
        Gun, Melee, Item
    }
    public class PlayerHotbar : MonoBehaviour, IContainerComponent
    {
        [field: SerializeField] public HotbarType[] HotbarTypes { get; private set; }
        [SerializeField] private SerializedDictionary<HotbarType, int> hotbarCount = new();

        private List<HotbarSlot> _slots = new();
        private PlayerEquipment _equipment;
        private PlayerInventory _inventory;
        private Player _player;
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>();
            _equipment = componentContainer.Get<PlayerEquipment>();
            _inventory = componentContainer.Get<PlayerInventory>();

            int total = 0;
            
            foreach (var kvp in hotbarCount)
            {
                for (int i = total; i < total + kvp.Value; i++)
                {
                    var hotbar = new HotbarSlot(null);
                    hotbar.SetOwner(_inventory);
                    hotbar.HotbarType = kvp.Key;
                    hotbar.Index = i;
                    _slots.Add(hotbar);
                }

                total += kvp.Value;
            }

            _player.PlayerInput.OnItemUsePressed += UseSlot;
            _inventory.InventoryChanged += HandleInventoryChanged;
            
            EventBus.Subscribe<EquipHotbarEvent>(HandleEquipHotbar);
            EventBus.Subscribe<UnEquipHotbarEvent>(HandleUnEquipHotbar);
            EventBus.Subscribe<HotbarUseEvent>(HandleUseHotbar);
        }

        private void OnDestroy()
        {
            if (_player != null)
                _player.PlayerInput.OnItemUsePressed -= UseSlot;

            if (_inventory != null)
                _inventory.InventoryChanged -= HandleInventoryChanged;
            
            EventBus.Unsubscribe<EquipHotbarEvent>(HandleEquipHotbar);
            EventBus.Unsubscribe<UnEquipHotbarEvent>(HandleUnEquipHotbar);
            EventBus.Unsubscribe<HotbarUseEvent>(HandleUseHotbar);
        }

        private void Start()
        {
            SyncHotbarSlots();
            UpdateUI();
        }

        private void HandleUseHotbar(HotbarUseEvent evt)
        {
            UseSlot(evt.Index);
        }

        private void HandleEquipHotbar(EquipHotbarEvent evt)
        {
            if (!IsValidIndex(evt.Index))
                return;

            if (evt.Item is not EquipableItem || !CheckValidItem(evt.Index, evt.Item))
                return;

            _slots[evt.Index].SetData(evt.Item, GetHotbarStack(evt.Item));
            UpdateUI();
        }

        private void HandleUnEquipHotbar(UnEquipHotbarEvent evt)
        {
            if (!IsValidIndex(evt.Index))
                return;

            _slots[evt.Index].SetData(null);
            UpdateUI();
        }
        
        public void UseSlot(int index)
        {
            if (!IsValidIndex(index) || _player.StateMachine.CurrentStateEnum == PlayerStateEnum.ItemUse)
                return;

            if (!TryResolveSlot(index, out EquipableItem equipable))
                return;

            _equipment.ChangeHandlingHotbarItem(equipable as Weapon);
            if (equipable is IUsable)
                _player.ChangeState(PlayerStateEnum.ItemUse);
        }
        
        private void HandleInventoryChanged()
        {
            if (SyncHotbarSlots())
                UpdateUI();
        }

        private bool SyncHotbarSlots()
        {
            bool isChanged = false;

            foreach (var slot in _slots)
            {
                isChanged |= SyncHotbarSlot(slot);
            }

            return isChanged;
        }

        private bool SyncHotbarSlot(HotbarSlot slot)
        {
            if (slot.Item == null)
                return false;

            if (!CheckValidItem(slot.Index, slot.Item))
            {
                slot.SetData(null);
                return true;
            }

            if (slot.Item is not UsableItem and not ThrowableItem)
            {
                if (slot.Stack == 1)
                    return false;

                slot.SetData(slot.Item, 1);
                return true;
            }

            ItemSlot inventorySlot = FindInventorySlot(slot.Item);
            if (inventorySlot == null)
            {
                slot.SetData(null);
                return true;
            }

            int stack = GetHotbarStack(inventorySlot.Item);
            if (slot.Item == inventorySlot.Item && slot.Stack == stack)
                return false;

            slot.SetData(inventorySlot.Item, stack);
            return true;
        }

        private bool TryResolveSlot(int index, out EquipableItem equipable)
        {
            equipable = null;
            var slot = _slots[index];

            bool isUpdated = SyncHotbarSlot(slot);
            if (isUpdated)
                UpdateUI();

            if (slot.Item is not EquipableItem validItem)
                return false;

            equipable = validItem;
            return true;
        }

        private ItemSlot FindInventorySlot(ItemBase item)
        {
            if (item == null)
                return null;

            foreach (var slot in _inventory.GetItemSlots(item.ItemData))
            {
                if (slot.Item == item)
                    return slot;
            }

            foreach (var slot in _inventory.GetItemSlots(item.ItemData))
            {
                if (slot.Item != null)
                    return slot;
            }

            return null;
        }

        private int GetHotbarStack(ItemBase item)
        {
            if (item is UsableItem or ThrowableItem)
                return Mathf.Max(1, _inventory.GetItemCount(item.ItemData));

            return 1;
        }

        private bool IsValidIndex(int index) => HotbarTypes != null && index >= 0 && index < _slots.Count && index < HotbarTypes.Length;

        private void UpdateUI() => EventBus.Raise(new UpdateHotbarUIEvent(_slots));
        public bool CheckValidItem(int index, ItemBase item) => IsValidIndex(index) && HotbarTypes[index] switch
        {
            HotbarType.Gun => item is GunItem,
            HotbarType.Melee => item is MeleeWeaponItem,
            HotbarType.Item => item is UsableItem or ThrowableItem,
            _ => false
        };
    }
}
