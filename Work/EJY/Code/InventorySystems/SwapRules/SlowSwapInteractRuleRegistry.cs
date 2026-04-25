using System.Collections.Generic;

namespace Code.InventorySystems.SwapRules
{
    public static class SlotSwapInteractRuleRegistry
    {
        public static List<ISlotSwapInteractRule> Create()
        {
            return new List<ISlotSwapInteractRule>
            {
                new EquipToEquipSwapRule(),
                new EquipToEmptySlotSwapRule(),
                new ItemToEquipSwapRule(),
                new HotbarToEmptySlotSwapRule(),
                new ItemToHotbarSwapRule(),
                new DefaultSlotSwapRule(),
            };
        }
    }
}