using System.Collections.Generic;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using UnityEngine;
using Code.InventorySystems.SwapRules;

namespace Code.InventorySystems
{
    public class InventorySwapInteract : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private SoundID dragEndSound;
        public ComponentContainer ComponentContainer { get; set; }
        private Inventory _inventory;
        private List<ISlotSwapInteractRule> _rules;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _inventory = componentContainer.GetSubclassComponent<Inventory>();
            _rules = SlotSwapInteractRuleRegistry.Create();
            
            EventBus.Subscribe<SwapItemSlotEvent>(HandleSwapItemSlot);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<SwapItemSlotEvent>(HandleSwapItemSlot);
        }

        private static void UpdateRelatedInventories(ItemSlot startSlot, ItemSlot targetSlot)
        {
            startSlot.OwnerInventory?.UpdateInventory();

            if (targetSlot.OwnerInventory != null && targetSlot.OwnerInventory != startSlot.OwnerInventory)
                targetSlot.OwnerInventory.UpdateInventory();
        }

        private void HandleSwapItemSlot(SwapItemSlotEvent evt)
        {
            ItemSlot startSlot = evt.StartSlot;
            ItemSlot targetSlot = evt.TargetSlot;

            if (startSlot == null || targetSlot == null)
            {
                Debug.Log("start slot or target slot is null");
                return;
            }

            SwapContext context = new SwapContext(startSlot, targetSlot);
            
            if (context.IsSameSlot || context.IsStartBlank)
                return;

            foreach (ISlotSwapInteractRule rule in _rules)
            {
                if (!rule.CanInteract(context))
                    continue;

                rule.Interact(context);
                BroAudio.Play(dragEndSound);
                UpdateRelatedInventories(startSlot, targetSlot);
                break;
            }
        }
    }
}