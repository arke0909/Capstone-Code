using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems;
using System.Collections.Generic;
using Code.AirDrop;
using Code.InventorySystems.Equipments;
using Code.Items;
using Code.Items.ItemInfo;
using Code.Players;
using Scripts.Entities;

namespace Code.ItemContainers
{
    public class ItemContainerInventory : Inventory
    {
        private bool _isSubscribe = false;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUIEvent);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUIEvent);
            base.OnDestroy();
        }

        public void OpenLootUI()
        {
            HandleSubscribe();
            UpdateInventory();
        }

        public void SetUpItem(List<ItemDataSO> items)
        {
            ClearInventory();

            for (int i = 0; i < items.Count && i < CurrentInventorySize; ++i)
            {
                var createData = items[i].CreateItem();
                itemSlots[i].SetData(createData.Item, createData.Stack);
                //Debug.Log($"{gameObject.name}에 {items[i].name} 아이템 들어감");
            }

            UpdateInventory();
        }

        public void SetUpRewards(List<SupplyReward> rewards)
        {
            ClearInventory();

            if (rewards == null)
            {
                UpdateInventory();
                return;
            }

            for (int i = 0; i < rewards.Count && i < CurrentInventorySize; ++i)
            {
                SupplyReward reward = rewards[i];
                if (reward.ItemData == null || reward.Stack <= 0)
                    continue;

                var createData = reward.ItemData.CreateItem();
                itemSlots[i].SetData(createData.Item, reward.Stack);
            }

            UpdateInventory();
        }

        public void SetUpItemSelf(List<SelfInitInfo> items)
        {
            ClearInventory();

            for (int i = 0; i < items.Count && i < CurrentInventorySize; ++i)
            {
                var createData = items[i].itemData.CreateItem();
                itemSlots[i].SetData(createData.Item, items[i].spawnCount);
                //Debug.Log($"{gameObject.name}에 {items[i].name} 아이템 들어감");
            }

            UpdateInventory();
        }

        public void SetUpItem(ItemDataSO item)
        {
            ClearInventory();

            var createData = item.CreateItem();
            itemSlots[0].SetData(createData.Item, createData.Stack);

            UpdateInventory();
        }

        private void HandleSubscribe()
        {
            if (!_isSubscribe)
            {
                InventoryChanged += UpdateUI;
                _isSubscribe = true;
            }
        }

        private void HandleUnsubscribe()
        {
            if (_isSubscribe)
            {
                InventoryChanged -= UpdateUI;
                _isSubscribe = false;
            }
        }

        private void UpdateUI()
        {
            EventBus.Raise(new UpdateRightInventoryUIEvent { ItemSlots = itemSlots, SlotCnt = CurrentInventorySize });
        }

        private void HandlePlayerUIEvent(PlayerUIEvent evt)
        {
            if (!evt.IsEnabled)
                HandleUnsubscribe();
        }
    }
}
