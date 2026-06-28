using Chipmunk.GameEvents;
using Code.ItemContainers;
using UnityEngine;

namespace Work.Code.GameEvents
{
    public struct AirdropEvent : IEvent
    {
        public int Area { get; }
        public Vector3 Position { get; }
        public ItemContainerInventory AirDropContainer { get; }

        public AirdropEvent(int Area, Vector3 Position, ItemContainerInventory airDropContainer)
        {
            this.Area = Area;
            this.Position = Position;
            this.AirDropContainer = airDropContainer;
        }
    }
}