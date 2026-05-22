using Chipmunk.Library.Utility.GameEvents.Local;
using Code.Items.ItemInfo;

namespace Work.Code.GameEvents
{
    public struct CompleteCraftingEvent : ILocalEvent
    {
        public ItemDataSO CraftedItem { get; }

        public CompleteCraftingEvent(ItemDataSO craftedItem)
        {
            CraftedItem = craftedItem;
        }
    }
    
    public struct StartCraftingEvent : ILocalEvent { }
}