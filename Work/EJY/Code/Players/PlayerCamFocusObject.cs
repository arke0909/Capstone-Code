using System;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using DewmoLib.Dependencies;
using SHS.Scripts.Crosshairs;
using Scripts.Players;
using UnityEngine;

namespace Code.Players
{
    public class PlayerCamFocusObject : MonoBehaviour, IContainerComponent
    {
        public float edgeSize = 20f;
        public float moveSpeed = 15f;
        public float smoothTime = 0.1f;

        private bool _isMove = false;

        public ComponentContainer ComponentContainer { get; set; }
        private CrosshairBehavior _crosshairManager;
        private Player _player;
        
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>();
            _crosshairManager = componentContainer.Get<CrosshairBehavior>();
            
            _player.PlayerInput.OnCameraLockPressed += HandleCameraLock;
        }

        private void OnDestroy()
        {
            _player.PlayerInput.OnCameraLockPressed -= HandleCameraLock;
        }

        private Vector3 velocity;

        private void HandleCameraLock(bool value)
        {
            if (!value)
            {
                transform.position = _player.transform.position;
                transform.SetParent(_player.transform);
            }
            else
            {
                transform.SetParent(null);
            }
            _isMove = value;
            Bus.Raise(new ChangeCameraFocus{TargetTrm = value ? transform : _player.transform });
        }
        
        private void Update()
        {
            if (!_isMove) return;

            Vector3 dir = Vector3.zero;
            Vector3 mousePos = _crosshairManager.GetCrosshairScreenPosition();

            if (mousePos.x <= edgeSize) dir.x = -1;
            else if (mousePos.x >= Screen.width - edgeSize) dir.x = 1;

            if (mousePos.y <= edgeSize) dir.z = -1;
            else if (mousePos.y >= Screen.height - edgeSize) dir.z = 1;

            if (dir == Vector3.zero) return;

            Transform camTrm = Camera.main.transform;
            float cameraYRot = camTrm.eulerAngles.y;

            Vector3 inputDir = new Vector3(dir.x, 0, dir.z);
            if (inputDir.sqrMagnitude > 1f)
                inputDir.Normalize();

            Vector3 moveDir =
                Quaternion.Euler(0, cameraYRot, 0) * inputDir;

            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

    }
}