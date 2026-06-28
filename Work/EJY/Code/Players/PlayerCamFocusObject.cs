using System;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
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

        private bool _isMove;
        private Vector3 _velocity;

        public ComponentContainer ComponentContainer { get; set; }
        private CrosshairBehavior _crosshairManager;
        private Player _player;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _player = componentContainer.Get<Player>();
            _crosshairManager = componentContainer.Get<CrosshairBehavior>();

            _player.PlayerInput.OnCameraLockPressed += HandleCameraLock;
            transform.SetParent(_player.transform);
            transform.position = _player.transform.position;
        }

        private void Start()
        {
            Bus.Raise(new ChangeCameraFocus { TargetTrm = transform });
        }

        private void OnDestroy()
        {
            _player.PlayerInput.OnCameraLockPressed -= HandleCameraLock;
        }

        private void HandleCameraLock(bool value)
        {
            if (!value)
                transform.SetParent(_player.transform);
            else
                transform.SetParent(null);

            _isMove = value;
            Bus.Raise(new ChangeCameraFocus { TargetTrm = transform });
        }

        private void Update()
        {
            if (!_isMove)
            {
                UpdateAimFocusPosition();
                return;
            }

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

        private void UpdateAimFocusPosition()
        {
            Vector3 targetPosition = _player.transform.position;
            CrosshairSO crosshairData = _crosshairManager.CurrentCrosshairData;

            if (_crosshairManager.IsCursorLocked && crosshairData != null)
            {
                Vector3 direction = _crosshairManager.GetAimPosition() - _player.transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    float focusWeight = GetScreenFocusWeight();
                    targetPosition += direction.normalized * crosshairData.cameraFocusDistance * focusWeight;
                }
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _velocity,
                smoothTime);
        }

        private float GetScreenFocusWeight()
        {
            Vector2 screenPosition = _crosshairManager.GetCrosshairScreenPosition();
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 halfScreenSize = new Vector2(
                Mathf.Max(1f, Screen.width * 0.5f),
                Mathf.Max(1f, Screen.height * 0.5f));

            Vector2 normalizedOffset = new Vector2(
                (screenPosition.x - screenCenter.x) / halfScreenSize.x,
                (screenPosition.y - screenCenter.y) / halfScreenSize.y);

            float normalizedDistance = Mathf.Clamp01(normalizedOffset.magnitude);
            return Mathf.SmoothStep(0f, 1f, normalizedDistance);
        }
    }
}
