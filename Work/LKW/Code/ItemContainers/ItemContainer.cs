using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems;
using EPOOutline;
using Scripts.Entities;
using Scripts.GameSystem;
using System.Collections.Generic;
using UnityEngine;
using Work.Code.UI;
using Work.LKW.Code.Events;
using Work.LKW.Code.Items.ItemInfo;
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
    public class ItemContainer : InteractableStructure,IContainerComponent
    {
        [SerializeField] private List<ItemType> allowedTypes;
        [field: SerializeField] public SpawnArea AllowedSpawnArea { get; private set; }
        [field: SerializeField] public SpawnSection SpawnSection { get; private set; }
        [SerializeField] private LayerMask whatIsPlayer;
        [SerializeField] private int minItems = 1;
        [SerializeField] private int maxItems = 4;
        public ItemContainerInventory Inventory { get; private set; }
        public ComponentContainer ComponentContainer { get; set; }


        public void OnInitialize(ComponentContainer componentContainer)
        {
            Inventory = componentContainer.Get<ItemContainerInventory>();
        }

        public List<ItemType> GetAllowedTypes() => allowedTypes;
        public int GetRandomCount() => Random.Range(minItems, maxItems + 1);

        [ContextMenu("Interact")]
        public override void Interact(Entity interactor)
        {
            Inventory.Select();
        }

    }
}
