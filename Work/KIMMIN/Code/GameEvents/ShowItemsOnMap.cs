using System.Collections.Generic;
using Chipmunk.GameEvents;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.GameEvents
{
    public struct ShowItemsOnMap : IEvent
    {
        public List<ItemDataSO> ItemList { get; }

        public ShowItemsOnMap(List<ItemDataSO> ItemList)
        {
            this.ItemList = ItemList;
        }
    }
}