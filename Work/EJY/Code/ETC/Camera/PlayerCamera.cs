using System;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Unity.Cinemachine;
using UnityEngine;

namespace Work.EJY.Code.ETC
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cam;

        private void Awake()
        {
            Bus.Subscribe<ChangeCameraFocus>(HandleChangeCameraFocus);
        }

        private void OnDestroy()
        {
            Bus.Unsubscribe<ChangeCameraFocus>(HandleChangeCameraFocus);
            
        }

        private void HandleChangeCameraFocus(ChangeCameraFocus evt)
        {
            cam.Target.TrackingTarget = evt.TargetTrm;
        }
    }
}