using Chipmunk.GameEvents;
using Code.GameEvents;

namespace Code.InventorySystems.SwapRules
{
    public class HotbarToEmptySlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartHotbar;
        }

        public void Interact(SwapContext context)
        {
            if (context.IsTargetBlank && context.IsSameInventory)
                EventBus.Raise(new UnEquipHotbarEvent(context.StartLocalIndex));
        }
    }
}