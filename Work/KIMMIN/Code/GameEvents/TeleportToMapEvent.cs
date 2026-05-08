using Chipmunk.GameEvents;
using UnityEngine;

namespace Work.Code.GameEvents
{
    public struct TeleportToMapEvent : IEvent
    {
        public int Area { get; }
        public Vector3 Position { get; }
        public Vector3 TargetPosition { get; }

        public TeleportToMapEvent(int area, Vector3 position, Vector3 targetPosition)
        {
            Area = area;
            Position = position;
            TargetPosition = targetPosition;
        }
    }
}
