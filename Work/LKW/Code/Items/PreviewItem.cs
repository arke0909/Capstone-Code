using System;
using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using DewmoLib.ObjectPool.RunTime;
using EPOOutline;
using Scripts.Entities;
using UnityEngine;
using Code.ItemContainers;

namespace Code.Items
{
public class PreviewItem : MonoBehaviour, IPoolable, IInteractable
{
    [SerializeField] private PoolItemSO viewItemPool;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private int _stack;
    private Camera _mainCamera;
    
    public ItemBase Item { get; private set; }
    [field: SerializeField] public Outlinable Outlinable { get; private set; }
    
    [Header("Pool")]
    private Pool _myPool;
    public PoolItemSO PoolItem => viewItemPool;
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        _mainCamera = Camera.main;
        Debug.Assert(spriteRenderer != null, "spriteRenderer 미할당", this);
        Debug.Assert(Outlinable != null, "Outlinable 미할당", this);
    }

    private void Init(ItemBase item, int stack)
    {
        Item = item;
        _stack = stack;

        spriteRenderer.sprite = Item.ItemData.itemImage;
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

    public void Select()
       => Outlinable.enabled = true;

    public void DeSelect()
       => Outlinable.enabled = false;

    public void Interact(Entity interactor)
    {
        if (interactor.TryGetSubclassComponent<Inventory>(out var inventory) 
            && inventory.TryAddItem(Item, _stack))
        {
            Item = null;
            _myPool.Push(this);
        }
    }
    
    #region Pooling
    public void SetUpPool(Pool pool)
        =>_myPool = pool;

    public void ResetItem()
    {
        Item = null;
        _stack = 0;
        
        Outlinable.enabled = false;
        spriteRenderer.sprite = null;
    }
    #endregion
}
}