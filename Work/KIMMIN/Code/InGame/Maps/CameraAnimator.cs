using System;
using Unity.Cinemachine;
using UnityEngine;
using Styles = Unity.Cinemachine.CinemachineBlendDefinition.Styles;

namespace Work.Code.Map
{
    public class CameraAnimator : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera targetCamera;
        [SerializeField] private float blendDuration;
        [SerializeField] private Styles blendStyle = Styles.EaseIn;

        private CinemachineBrain _brain;
        private float _startTime;

        private void Awake()
        {
            _brain = Camera.main.GetComponent<CinemachineBrain>();
            Debug.Assert(_brain != null, "CinemachineBrain not found");
        }

        private void Update()
        {
            if (Time.time - _startTime > blendDuration)
            {
                DisableCamera();
            }
        }

        public void EnableCamera()
        {
            _brain.DefaultBlend.Time = blendDuration;
            _brain.DefaultBlend.Style = blendStyle;
            _startTime = Time.time;
            
            targetCamera.Priority = 100;
        }

        public void DisableCamera()
        {
            targetCamera.Priority = -1;
        }
    }
}