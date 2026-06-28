using Chipmunk.Library.Utility.GameEvents.Local;
using Code.Items.ItemInfo;

namespace Work.Code.GameEvents
{
    public struct CompleteCraftingEvent : ILocalEvent
    {
        public ItemDataSO CraftedItem { get; }
        public ItemDataSO[] AutoCraftedItems { get; }

        public CompleteCraftingEvent(ItemDataSO craftedItem, ItemDataSO[] autoCraftedItems = null)
        {
            CraftedItem = craftedItem;
            AutoCraftedItems = autoCraftedItems;
        }

        public bool ContainsCraftedItem(ItemDataSO item)
        {
            if (CraftedItem == item)
                return true;

            if (AutoCraftedItems == null)
                return false;

            for (int i = 0; i < AutoCraftedItems.Length; i++)
            {
                if (AutoCraftedItems[i] == item)
                    return true;
            }

            return false;
        }
    }
    
    public struct StartCraftingEvent : ILocalEvent { }
}
