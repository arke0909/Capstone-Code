namespace Code.InventorySystems.SwapRules
{
    public class ItemCountMergeRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartStorage &&
                   context.IsTargetStorage &&
                   !context.IsSameSlot &&
                   context.StartItem != null &&
                   context.TargetItem != null &&
                   !context.TargetSlot.IsFull &&
                   context.StartItem.ItemData == context.TargetItem.ItemData;
        }

        public void Interact(SwapContext context)
        {
            int moved = context.TargetInventory.AddItemToSlot(
                context.TargetSlot,
                context.StartItem,
                context.StartSlot.Stack);

            if (moved > 0)
                context.StartInventory.RemoveItem(context.StartItem, moved, false);
        }
    }
}
