using System;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.ETC
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource impulseSource;
        
        private void Awake()
        {
            Bus.Subscribe<CameraShakeEvent>(HandleCameraShake);
        }

        private void OnDestroy()
        {
            Bus.Unsubscribe<CameraShakeEvent>(HandleCameraShake);
        }

        private void HandleCameraShake(CameraShakeEvent evt)
        {
            impulseSource.GenerateImpulseAtPositionWithVelocity(evt.ImpulsePosition, evt.Velocity);
        }
    }
}