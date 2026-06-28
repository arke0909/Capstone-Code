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
        [SerializeField] private float zoomSmoothTime = 0.12f;

        private float _baseFieldOfView;
        private float _targetFieldOfView;
        private float _fieldOfViewVelocity;

        private void Awake()
        {
            Debug.Assert(cam != null, "PlayerCamera requires CinemachineCamera.");
            _baseFieldOfView = cam.Lens.FieldOfView;
            _targetFieldOfView = _baseFieldOfView;

            Bus.Subscribe<ChangeCameraFocus>(HandleChangeCameraFocus);
            Bus.Subscribe<ChangeCameraZoom>(HandleChangeCameraZoom);
        }

        private void OnDestroy()
        {
            Bus.Unsubscribe<ChangeCameraFocus>(HandleChangeCameraFocus);
            Bus.Unsubscribe<ChangeCameraZoom>(HandleChangeCameraZoom);
        }

        private void Update()
        {
            if (Mathf.Approximately(cam.Lens.FieldOfView, _targetFieldOfView))
                return;

            cam.Lens.FieldOfView = Mathf.SmoothDamp(
                cam.Lens.FieldOfView,
                _targetFieldOfView,
                ref _fieldOfViewVelocity,
                zoomSmoothTime);
        }

        private void HandleChangeCameraFocus(ChangeCameraFocus evt)
        {
            cam.Target.TrackingTarget = evt.TargetTrm;
        }

        private void HandleChangeCameraZoom(ChangeCameraZoom evt)
        {
            Debug.Assert(evt.FieldOfViewReduction >= 0f, "FieldOfViewReduction must be non-negative.");
            Debug.Assert(evt.FieldOfViewReduction < _baseFieldOfView, "FieldOfViewReduction must be smaller than base field of view.");

            _targetFieldOfView = _baseFieldOfView - evt.FieldOfViewReduction;
        }
    }
}
