using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.Items.ItemInfo;

namespace Work.Code.GameEvents
{
    public struct ShowItemsOnMap : IEvent
    {
        public ItemDataSO[] ItemList { get; }

        public ShowItemsOnMap(ItemDataSO[] ItemList)
        {
            this.ItemList = ItemList;
        }
    }

    public struct HideItemsOnMap : IEvent
    {
        public List<ItemDataSO> ItemList { get; }

        public HideItemsOnMap(List<ItemDataSO> ItemList)
        {
            this.ItemList = ItemList;
        }
    }
}