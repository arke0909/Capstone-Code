using Chipmunk.GameEvents;
using UnityEngine;

namespace Work.Code.GameEvents
{
    public struct DamageTextEvent : IEvent
    {
        public float Damage { get; }
        public Vector3 Position { get; }

        public DamageTextEvent(float damage, Vector3 position)
        {
            Damage = damage;
            Position = position;
        }
    }
}