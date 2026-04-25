namespace Code.InventorySystems
{
    public interface ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context);
        
        public void Interact(SwapContext context);
    }
}