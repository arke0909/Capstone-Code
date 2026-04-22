using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.NoiseSystems.Events
{
    public struct NoiseListenedEvent : ILocalEvent
    {
        public Entity Source { get; }
        public float NoiseVolume { get; }
        public Vector3 NoisePosition { get; }

        public NoiseListenedEvent(Entity Source, Vector3 noisePosition, float noiseVolume)
        {
            this.Source = Source;
            NoisePosition = noisePosition;
            NoiseVolume = noiseVolume;
        }
    }
}