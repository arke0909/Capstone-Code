using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace Code.ETC.MapObjects
{
    public readonly struct ItemDropEvent : ILocalEvent
    {
        public Vector3 From { get; }
        public Vector3 To { get; }

        public ItemDropEvent(Vector3 from, Vector3 to)
        {
            From = from;
            To = to;
        }
    }
}
