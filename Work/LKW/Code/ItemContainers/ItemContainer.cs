using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Work.LKW.Code.Events;
using Code.GameEvents;
using Code.InventorySystems;
using Code.InventorySystems.Items;
using Work.LKW.Code.Items.ItemInfo;
using EPOOutline;
using Scripts.Entities;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.UI;
using Work.Code.UI.Misc;
using Random = UnityEngine.Random;

namespace Work.LKW.Code.ItemContainers
{
    public interface IInteractable
    {
        public void Select();
        public void DeSelect();
        public void Interact(Entity interactor);
        
        public Outlinable Outlinable { get; }
            
    }
    public class ItemContainer : Inventory, IInteractable
    {
        [SerializeField] private List<ItemType> allowedTypes;
        [field:SerializeField] public SpawnArea AllowedSpawnArea {get; private set;}
        [field:SerializeField] public SpawnSection SpawnSection { get; private set; }
        [SerializeField] private LayerMask whatIsPlayer;
        [SerializeField] private int minItems = 1;
        [SerializeField] private int maxItems = 4;
        [SerializeField] private AppearEffect helpText;
        
        [field: SerializeField] public Outlinable Outlinable { get; private set; }
        
        private bool _isSubscribe = false;
        private bool _isSelected = false;

        private Camera _cam;

        protected override void Awake()
        {
            base.Awake();
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUIEvent);
            _cam = Camera.main;
        }

        protected override void CreateSlot(int idx)
        {
            idx += (int)SlotType.ItemContainer;
            base.CreateSlot(idx);
        }

        private void Start()
        {
            Outlinable.enabled = false;
            helpText.Disappear();
        }

        private void LateUpdate()
        {
            if (_isSelected)
            {
                helpText.transform.forward = _cam.transform.forward;
            }
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUIEvent);
            base.OnDestroy();
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


        public List<ItemType> GetAllowedTypes() => allowedTypes;
        public int GetRandomCount() => Random.Range(minItems, maxItems + 1);

        [ContextMenu("Interact")]
        public void Interact(Entity interactor)
        {
            EventBus.Raise( new OpenPlayerUIEvent(true));
            var evt = new OpenItemContainerEvent(this);
            Bus.Raise(evt);

            HandleSubscribe();
            UpdateInventory();
        }

        private void HandleSubscribe()
        {
            if(!_isSubscribe)
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
        
        private void HandlePlayerUIEvent(PlayerUIEvent evt)
        {
            if(!evt.IsEnabled)
                HandleUnsubscribe();
        }
        private void UpdateUI()
        {
            EventBus.Raise(new UpdateInventoryUIEvent { ItemSlots = itemSlots, isPlayerInventory = false, SlotCnt = CurrentInventorySize });
        }

        public void Select()
        {
            _isSelected = true;
            helpText.Appear();
            Outlinable.enabled = true;
        }

        public void DeSelect()
        {
            helpText.Disappear();
            _isSelected = false;
            Outlinable.enabled = false;
        }
    }
}