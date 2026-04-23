using Chipmunk.GameEvents;
using UnityEngine;

namespace Work.Code.GameEvents
{
    public struct AirdropEvent : IEvent
    {
        public int Area { get; }
        public Vector3 Position { get; }

        public AirdropEvent(int Area, Vector3 Position)
        {
            this.Area = Area;
            this.Position = Position;
        }
    }
}