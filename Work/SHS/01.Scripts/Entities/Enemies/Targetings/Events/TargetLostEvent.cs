using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Targetings.Events
{
    public struct TargetLostEvent : ILocalEvent
    {
        public Vector3 LastKnownPosition { get; }

        public TargetLostEvent(Vector3 lastKnownPosition)
        {
            LastKnownPosition = lastKnownPosition;
        }
    }
}