using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;

namespace Code.InventorySystems.SwapRules
{
    public class EquipToOccupiedItemSlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartEquip &&
                   !context.IsTargetBlank &&
                   context.IsTargetStorage &&
                   context.StartEquipSlot.CanEquip(context.TargetItem);
        }

        public void Interact(SwapContext context)
        {
            // Nothing
        }
    }
}
