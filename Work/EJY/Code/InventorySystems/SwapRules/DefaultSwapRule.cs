namespace Code.InventorySystems.SwapRules
{
    public class DefaultSlotSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context) => true;

        public void Interact(SwapContext context)
        {
            int targetStack = context.TargetSlot.Stack;
            int startStack = context.StartSlot.Stack;

            context.StartSlot.SetData(context.TargetItem, targetStack);
            context.TargetSlot.SetData(context.StartItem, startStack);
        }
    }
}