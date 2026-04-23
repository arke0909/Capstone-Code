using System;
using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using Code.Players;
using DewmoLib.ObjectPool.RunTime;
using EPOOutline;
using Scripts.Entities;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.ItemContainers;

namespace Work.LKW.Code.Items
{
    public class PreviewItem : MonoBehaviour, IPoolable, IInteractable
    {
        [SerializeField] private PoolItemSO viewItemPool;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private LayerMask whatIsPlayer;
        
        [SerializeField] private GameObject interactCircle;
        [field: SerializeField] public Outlinable Outlinable { get; private set; }
        
        private ItemBase _item;
        private int _stack;
        private Pool _myPool;
        
        public PoolItemSO PoolItem => viewItemPool;
        public GameObject GameObject => gameObject;
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            
        }
        
        private void OnEnable()
        {
            interactCircle.SetActive(false);
        }

        public void Init(ItemBase item, int stack)
        {
            if (item == null) return;

            _item = item;
            _stack = stack;
            
            if (item.ItemData.itemImage != null)
                spriteRenderer.sprite = _item.ItemData.itemImage;
            gameObject.name = $"dropItem_{_item.ItemData.itemName}";

            Outlinable.enabled = false;
        }

        public void Discard(Vector3 dropPosition, ItemBase item, int stack)
        {
            Init(item, stack);
            
            transform.forward = -Camera.main.transform.forward;
            transform.position = dropPosition;
        }

        public void Handle(Sprite itemIcon)
        {
            spriteRenderer.sprite = itemIcon;
        }


        public void Select()
        {
            Outlinable.enabled = true;
        }

        public void DeSelect()
        {
            Outlinable.enabled = false;
        }

        public void Interact(Entity interactor)
        {
            if (interactor.TryGetSubclassComponent<Inventory>(out var inventory) && inventory.TryAddItem(_item, _stack))
            {
                _item = null;
                _myPool.Push(this);
            }
        }
    }
}