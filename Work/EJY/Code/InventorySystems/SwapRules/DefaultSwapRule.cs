using Code.InventorySystems.Items;

namespace Code.InventorySystems.SwapRules
{
    public class DefaultSlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context) 
            => !context.IsStartEquip &&
               !context.IsTargetEquip &&
               (context.StartSlotType == SlotType.Inventory || context.TargetSlotType == SlotType.Inventory ||
                context.StartSlotType == SlotType.ItemContainer || context.TargetSlotType == SlotType.ItemContainer);

        public void Interact(SwapContext context)
        {
            int targetStack = context.TargetSlot.Stack;
            int startStack = context.StartSlot.Stack;

            context.StartSlot.SetData(context.TargetItem, targetStack);
            context.TargetSlot.SetData(context.StartItem, startStack);
        }
    }
}
