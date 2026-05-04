using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems;
using System.Collections.Generic;
using Work.LKW.Code.Events;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.LKW.Code.ItemContainers
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
        public void Select()
        {
            EventBus.Raise(new OpenPlayerUIEvent(true));
            var evt = new OpenRightInventoryEvent(this);
            Bus.Raise(evt);

            HandleSubscribe();
            UpdateInventory();
        }
        public void SetUpItem(List<ItemDataSO> items)
        {
            for (int i = 0; i < items.Count && i < CurrentInventorySize; ++i)
            {
                var createData = items[i].CreateItem();
                itemSlots[i].SetData(createData.Item, createData.Stack);
                //Debug.Log($"{gameObject.name}에 {items[i].name} 아이템 들어감");
            }

            UpdateInventory();
        }

        public void SetUpItem(ItemDataSO item)
        {
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
