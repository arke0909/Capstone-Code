using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;

namespace Code.InventorySystems.SwapRules
{
    public class EquipToOccupiedItemSlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            bool isItemStorageTarget =
                context.TargetSlotType == SlotType.Inventory ||
                context.TargetSlotType == SlotType.ItemContainer;

            return context.IsStartEquip &&
                   !context.IsTargetBlank &&
                   isItemStorageTarget &&
                   context.StartEquipSlot.CanEquip(context.TargetItem);
        }

        public void Interact(SwapContext context)
        {
            // Nothing
        }
    }
}