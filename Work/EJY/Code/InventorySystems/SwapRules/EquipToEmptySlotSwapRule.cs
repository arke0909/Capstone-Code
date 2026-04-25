using Chipmunk.GameEvents;
using InGame.InventorySystem;

namespace Code.InventorySystems.SwapRules
{
    public class EquipToEmptySlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartEquip && context.IsTargetBlank;
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