using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;

namespace Code.InventorySystems.SwapRules
{
    public class ItemToEquipSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartStorage &&
                   context.IsTargetEquip &&
                   context.TargetEquipSlot.CanEquip(context.StartItem);
        }

        public void Interact(SwapContext context)
        {
            EventBus.Raise(new EquipByDragEvent(
                context.TargetLocalIndex,
                context.StartSlot));
        }
    }
}
