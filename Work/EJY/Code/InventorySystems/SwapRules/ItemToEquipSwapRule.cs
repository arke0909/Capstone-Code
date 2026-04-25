using Chipmunk.GameEvents;
using InGame.InventorySystem;

namespace Code.InventorySystems.SwapRules
{
    public class ItemToEquipSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsTargetEquip;
        }

        public void Interact(SwapContext context)
        {
            if (!context.TargetEquipSlot.CanEquip(context.StartItem))
                return;

            void OnEquipByDragSuccess()
            {
                if (context.StartInventory != context.TargetInventory)
                    context.StartInventory.RemoveItem(context.StartItem, 1, false);
            }

            EventBus.Raise(new EquipByDragEvent(
                context.StartItem,
                context.TargetLocalIndex,
                context.StartSlot,
                OnEquipByDragSuccess));
        }
    }
}