using Chipmunk.GameEvents;
using UnityEngine;

namespace Code.GameEvents
{
    public struct CameraShakeEvent : IEvent
    {
        public Vector3 ImpulsePosition;
        public Vector3 Velocity;

        public CameraShakeEvent(Vector3 impulsePosition, Vector3 velocity, float force)
        {
            ImpulsePosition = impulsePosition;
            Velocity = velocity * force;
        }
    }
}