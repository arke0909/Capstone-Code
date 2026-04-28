using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;

namespace Code.InventorySystems.SwapRules
{
    public class EquipToEmptySlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartEquip &&
                   context.IsTargetBlank &&
                   context.IsTargetStorage;
        }

        public void Interact(SwapContext context)
        {
            EventBus.Raise(new UnEquipByDragEvent(
                context.StartItem,
                context.StartEquipSlot,
                context.TargetSlot));
        }
    }
}
