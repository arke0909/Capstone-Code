using Chipmunk.GameEvents;
using Code.GameEvents;

namespace Code.InventorySystems.SwapRules
{
    public class EquipToEquipSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsStartEquip && context.IsTargetEquip;
        }

        public void Interact(SwapContext context)
        {
            if (context.TargetEquipSlot.CanEquip(context.StartItem) &&
                context.StartEquipSlot.CanEquip(context.TargetItem))
            {
                EventBus.Raise(new SwapEquipEvent(context.StartEquipSlot, context.TargetEquipSlot));
            }
        }
    }
}