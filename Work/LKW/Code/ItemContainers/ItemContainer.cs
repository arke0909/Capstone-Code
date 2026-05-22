using System;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems;
using EPOOutline;
using Scripts.Entities;
using Scripts.GameSystem;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.UI;
using Work.LKW.Code.Events;
using Code.Items.ItemInfo;
using Random = UnityEngine.Random;

namespace Code.ItemContainers
{
    public interface IInteractable
    {
        public void Select();
        public void DeSelect();
        public void Interact(Entity interactor);

        public Outlinable Outlinable { get; }
    }

    [Serializable]
    public struct SelfInitInfo
    {
        public ItemDataSO itemData;
        public int spawnCount;
    }
    
    public class ItemContainer : InteractableStructure,IContainerComponent
    {
        [Header("Item Spawn Setting")]
        [SerializeField] private List<ItemType> allowedTypes;
        [SerializeField] private int minItems = 1;
        [SerializeField] private int maxItems = 4;
        [field: SerializeField] public SpawnArea AllowedSpawnArea { get; private set; }
    
        [Header("Self Initialization")]
        [field:SerializeField] public bool IsSelfInitialized { get; private set; } = false;
        [SerializeField, ShowIf("IsSelfInitialized")] private List<SelfInitInfo> infoList = new List<SelfInitInfo>();
    
        public ItemContainerInventory Inventory { get; private set; }
        public ComponentContainer ComponentContainer { get; set; }
    
        protected override void Start()
        {
            base.Start();

            InitializeSelf();
        }
    
        public void OnInitialize(ComponentContainer componentContainer)
        {
            Inventory = componentContainer.Get<ItemContainerInventory>();
        }

        private void InitializeSelf()
        {
            if(IsSelfInitialized == false) return;
        
            if (Inventory == null)
            {
                Debug.LogError($"[ItemContainer] Inventory가 null입니다: {gameObject.name}");
                return;
            }
            Inventory.SetUpItemSelf(infoList);
        }

        public List<ItemType> GetAllowedTypes() => allowedTypes;
        public int GetRandomCount() => Random.Range(minItems, maxItems + 1);

        [ContextMenu("Interact")]
        public override void Interact(Entity interactor)
        {
            if (Inventory == null)
                return;

            EventBus.Raise(new OpenPlayerUIEvent(true));
            Bus.Raise(new OpenRightInventoryEvent(Inventory));
            Inventory.OpenLootUI();
        }

    }
}
