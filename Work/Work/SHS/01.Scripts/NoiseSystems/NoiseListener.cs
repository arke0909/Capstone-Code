using System;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using SHS.Scripts.NoiseSystems.Events;
using UnityEngine;

namespace SHS.Scripts.NoiseSystems
{
    public class NoiseListener : MonoBehaviour, INoiseListener, IContainerComponent
    {
        private LocalEventBus _localEventBus;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _localEventBus = componentContainer.Get<LocalEventBus>();
        }

        public Action<float> OnNoise;

        public void OnNoiseListen(NoiseData noiseData)
        {
            float distance = Vector3.Distance(transform.position, noiseData.Position);
            if (distance > noiseData.Radius)
                return;

            float volume = 1f - (distance / noiseData.Radius);
            OnNoise?.Invoke(volume);
            _localEventBus.Raise(new NoiseListenedEvent(noiseData.Source, noiseData.Position, volume));
        }
    }
}