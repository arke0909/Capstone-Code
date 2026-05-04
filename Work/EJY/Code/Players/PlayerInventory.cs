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
            int previousSize = (int)prevValue;
            CurrentInventorySize = (int)currentValue;

            if (CurrentInventorySize >= previousSize)
            {
                UpdateInventory();
                return;
            }

            for (int i = CurrentInventorySize; i < previousSize; i++)
            {
                ItemSlot overflowSlot = itemSlots[i];
                ItemBase overflowItem = overflowSlot.Item;
                int stack = overflowSlot.Stack;

                if (overflowItem == null)
                    continue;

                if (TryAddItem(overflowItem, stack))
                {
                    if (!ContainsItem(overflowItem))
                        overflowItem.SetOwner(null);

                    overflowSlot.Clear();
                    continue;
                }

                var poolPreviewItem = _poolManagerMono.Pop<PreviewItem>(previewItem);
                Vector3 discardPos = transform.position;

                discardPos.x += Random.Range(-1f, 1f);
                discardPos.z += Random.Range(-1f, 1f);
                discardPos.y += 0.2f;

                poolPreviewItem.Discard(discardPos, overflowItem, stack);
                overflowItem.SetOwner(null);
                overflowSlot.Clear();
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
            EventBus.Raise(new UpdateLeftInventoryUIEvent
                { ItemSlots = itemSlots, SlotCnt = CurrentInventorySize });
        }
    }
}