using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.Events;
using Unity.Cinemachine;
using UnityEngine;

namespace Code.ETC.CameraZone
{
    public class CameraZoneManager : MonoBehaviour
    {
        private CinemachineCamera _activeCamera;
        [SerializeField] private CinemachineBrain cinemachineBrain;

        private void OnEnable() => Bus.Subscribe<CameraSwitchEvent>(HandleCameraSwitch);
        private void OnDisable() => Bus.Unsubscribe<CameraSwitchEvent>(HandleCameraSwitch);

        private void HandleCameraSwitch(CameraSwitchEvent evt)
        {
            if (_activeCamera == evt.Data.TargetCamera) return;

            var blend = cinemachineBrain.DefaultBlend;
            blend.Time = evt.Data.BlendTime;
            cinemachineBrain.DefaultBlend = blend;
            
            if(_activeCamera != null)
                _activeCamera.Priority = 0;
            evt.Data.TargetCamera.Priority = 10;
            _activeCamera = evt.Data.TargetCamera;
        }

    }
}