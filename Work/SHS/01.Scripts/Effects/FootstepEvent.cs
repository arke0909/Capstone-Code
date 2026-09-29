using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public readonly struct FootstepEvent : ILocalEvent
    {
        public Vector3 Position { get; }
        public Vector3 Forward { get; }

        public FootstepEvent(Vector3 position, Vector3 forward)
        {
            Position = position;
            Forward = forward;
        }
    }
}
