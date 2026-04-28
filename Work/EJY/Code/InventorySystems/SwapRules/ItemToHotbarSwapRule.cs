using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using Scripts.Combat.Datas;
using Work.LKW.Code.Items;

namespace Code.InventorySystems.SwapRules
{
    public class ItemToHotbarSwapRule : ISlotSwapInteractRule
    {
        public bool CanInteract(SwapContext context)
        {
            return context.StartSlotType == SlotType.Inventory &&
                   context.IsTargetHotbar &&
                   context.IsTargetBlank &&
                   context.IsSameInventory &&
                   context.StartItem is ThrowableItem or UsableItem;
        }

        public void Interact(SwapContext context)
        {
            EventBus.Raise(new EquipHotbarEvent(context.TargetLocalIndex, context.StartItem));
        }
    }
}