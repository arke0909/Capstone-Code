using System;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.InventorySystems;
using DewmoLib.ObjectPool.RunTime;
using EPOOutline;
using Scripts.Entities;
using UnityEngine;
using Code.ItemContainers;
using Code.Items.ItemInfo;
using EPOOutline.Demo;
using Scripts.GameSystem;
using TMPro;
using Work.Code.GameEvents;

namespace Code.Items
{
    public class PreviewItem : InteractableStructure, IPoolable
    {
        [SerializeField] private PoolItemSO viewItemPool;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private SoundID pickupSound;

        private int _stack;
        private Camera _mainCamera;

        public ItemBase Item { get; private set; }

        [Header("Pool")] private Pool _myPool;
        public PoolItemSO PoolItem => viewItemPool;
        public GameObject GameObject => gameObject;

        protected override void Awake()
        {
            base.Awake();
            _mainCamera = Camera.main;
            Debug.Assert(spriteRenderer != null, "spriteRenderer 미할당", this);
            Debug.Assert(itemNameText != null, "itemNameText 미할당", this);
            Debug.Assert(Outlinable != null, "Outlinable 미할당", this);
        }

        private void Init(ItemBase item, int stack)
        {
            Item = item;
            _stack = stack;

            spriteRenderer.sprite = Item.ItemData.itemImage;
            spriteRenderer.enabled = spriteRenderer.sprite != null;

            itemNameText.text = Item.ItemData.itemName;
            itemNameText.enabled = !string.IsNullOrWhiteSpace(itemNameText.text);

            gameObject.name = $"dropItem_{Item.ItemData.itemName}";

            Outlinable.enabled = false;
        }

        public void Discard(Vector3 dropPosition, ItemBase item, int stack)
        {
            if (item != null)
            {
                Init(item, stack);
                transform.forward = -_mainCamera.transform.forward;
                transform.position = dropPosition;
            }
        }

        public override void Interact(Entity interactor)
        {
            if (interactor.TryGetSubclassComponent<Inventory>(out var inventory)
                && inventory.TryAddItem(Item, _stack))
            {
                ItemDataSO pickedUpItemData = Item.ItemData;
                Item = null;
                _myPool.Push(this);
                BroAudio.Play(pickupSound);
                EventBus.Raise(new ItemPickedUpEvent(pickedUpItemData));
            }
        }

        #region Pooling

        public void SetUpPool(Pool pool)
            => _myPool = pool;

        public void ResetItem()
        {
            Item = null;
            _stack = 0;

            Outlinable.enabled = false;
            spriteRenderer.enabled = false;
            spriteRenderer.sprite = null;

            itemNameText.enabled = false;
            itemNameText.text = string.Empty;
        }

        #endregion
    }
}
