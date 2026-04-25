using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Combat.Datas;
using Work.LKW.Code.Items;

namespace Code.InventorySystems.SwapRules
{
    public class ItemToHotbarSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.IsTargetHotbar;
        }

        public void Interact(SwapContext context)
        {
            if (context.IsTargetBlank &&
                context.IsSameInventory &&
                context.StartItem is ThrowableItem or UsableItem)
            {
                EventBus.Raise(new EquipHotbarEvent(context.TargetLocalIndex, context.StartItem));
            }
        }
    }
}