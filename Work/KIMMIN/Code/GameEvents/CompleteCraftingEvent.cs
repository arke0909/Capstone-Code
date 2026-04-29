using Chipmunk.Library.Utility.GameEvents.Local;
using Work.LKW.Code.Items.ItemInfo;

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