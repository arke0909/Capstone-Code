using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Chipmunk.Modules.StatSystem;
using Code.GameEvents;
using Code.InventorySystems;
using Code.InventorySystems.Items;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Code.Players
{
    public class PlayerInventory : Inventory,IAfterInitialze
    {
        [SerializeField] private StatSO invenSlotCountStat;
        [SerializeField] private PoolItemSO previewItem;
        [Inject] private PoolManagerMono _poolManagerMono;
        private StatOverrideBehavior _StatOverrideBehavior;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _StatOverrideBehavior = ComponentContainer.Get<StatOverrideBehavior>();

            InventoryChanged += UpdateUI;
        }

        public void AfterInitialize()
        {
            CurrentInventorySize =
                (int)_StatOverrideBehavior.SubscribeStat(invenSlotCountStat, HandleInvenSlotCount, CurrentInventorySize);
            UpdateInventory();
        }

        private void HandleInvenSlotCount(StatSO stat, float currentValue, float prevValue)
        {
            CurrentInventorySize = (int)currentValue;
            
            int gap = CurrentInventorySize - (int)prevValue;

            if (gap < 0)
            {
                gap = -gap;

                for (int i = CurrentInventorySize; i < CurrentInventorySize + gap; i++)
                {
                    ItemSlot slot = itemSlots[i];
                    ItemBase targetItem = slot.Item;
                    int stack = slot.Stack;
                    
                    if(targetItem == null) continue;

                    if (InventoryHasBlankSlot())
                    {
                        var blankSlot = GetItemSlot(null);
                        blankSlot.SetData(targetItem, stack);
                        slot.SetData(null);
                    }
                    else
                    {
                        var poolPreviewItem = _poolManagerMono.Pop<PreviewItem>(previewItem);
                        Vector3 discardPos = transform.position;

                        float x = Random.Range(-1f, 1f);
                        float z = Random.Range(-1f, 1f);
                        discardPos.x += x;
                        discardPos.z += z;
                        discardPos.y += 0.2f;
                    
                        poolPreviewItem.Discard(discardPos, targetItem, stack);
                        slot.RemoveItem(slot.Stack);
                    }
                }
            }
            
            UpdateInventory();
        }

        protected override void OnDestroy()
        {
            
            InventoryChanged -= UpdateUI;
        }

        private void Start()
        {
            UpdateInventory();
        }

        private void UpdateUI()
        {
            EventBus.Raise(new UpdateInventoryUIEvent
                { ItemSlots = itemSlots, isPlayerInventory = true, SlotCnt = CurrentInventorySize });
        }
    }
}