using System;
using Chipmunk.ComponentContainers;
using UnityEngine;
using UnityEngine.InputSystem;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using Scripts.Players;
using InGame.PlayerUI;
using Code.ETC;
using Code.GameEvents;
using Code.Players;
using Code.Items;
using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.InventorySystems.Equipments;
using DewmoLib.Dependencies;
using Scripts.Players.States;
using SHS.Scripts.Combats.Events;
using UnityEngine.Serialization;

namespace SHS.Scripts.Crosshairs
{
    [Provide]
    public class CrosshairBehavior : MonoBehaviour, IContainerComponent, IAfterInitialze, IAimProvider,
        IDependencyProvider
    {
        [Header("Crosshair Assets")] [SerializeField]
        private CrosshairSO defaultCrosshair;

        [Header("Input")] [SerializeField] private float baseSensitivity = 18f;
        [SerializeField] private float clampMargin = 8f;

        [Header("Aim Sampling")] [SerializeField]
        private LayerMask whatIsGround;

        [SerializeField] private float fallbackAimDistance = 100f;

        public ComponentContainer ComponentContainer { get; set; }
        public bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;
        public float CurrentSpreadRadiusPixels { get; private set; }
        public CrosshairSO CurrentCrosshairData { get; private set; }

        private Player _player;
        private PlayerEquipment _equipment;
        private LocalEventBus _localEventBus;

        private GunDataSO _currentGunData;
        private GunObject _currentGunObject;

        private Vector2 _userCursorPixel;
        private Vector2 _recoilTargetPixel;
        private Vector2 _recoilOffsetPixel;
        private Vector2 _recoilRecoveryVelocity;
        private Vector3 _aimPosition;
        private Vector3 _worldAimPosition;
        private float _lastShotTime = -999f;
        public float Sensitivity => _sensitivity * CurrentCrosshairData.sensitivity;
        private float _sensitivity;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;

            _player = componentContainer.Get<Player>(true);
            _equipment = componentContainer.Get<PlayerEquipment>();
            _localEventBus = componentContainer.Get<LocalEventBus>();

            _player.PlayerInput.OnCursorMoved += HandleCursorMove;
            _localEventBus.Subscribe<ChangeHandlingEvent>(HandleChangeHandling);
            _localEventBus.Subscribe<GunAttackEvent>(HandleGunAttack);

            _userCursorPixel = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        public void AfterInitialize()
        {
            RefreshEquippedGunContext();
            _worldAimPosition = GetCrosshairWorldPosition();
            _aimPosition = GetCrosshairPlanePosition();
            UpdateSpreadRadiusPixels();
        }

        private void OnDestroy()
        {
            _player.PlayerInput.OnCursorMoved -= HandleCursorMove;

            _localEventBus.Unsubscribe<ChangeHandlingEvent>(HandleChangeHandling);
            _localEventBus.Unsubscribe<GunAttackEvent>(HandleGunAttack);
        }

        private void Update()
        {
            if (IsCursorLocked)
            {
                RecoverRecoilTarget();
                UpdateRecoilOffset();
            }
            else
            {
                MoveCursorToMouse();
            }

            UpdateSpreadRadiusPixels();
            UpdateCurrentCrosshairData();
        }

        private void LateUpdate()
        {
            _worldAimPosition = GetCrosshairWorldPosition();

            Vector3 nextAimPosition = GetCrosshairPlanePosition();
            Vector3 planarOffset = nextAimPosition - transform.position;
            planarOffset.y = 0f;

            // if (planarOffset.sqrMagnitude < minAimDistance * minAimDistance)
            // return;

            _aimPosition = nextAimPosition;
        }

        public Vector3 GetAimPosition()
            => _aimPosition;

        public Vector3 GetAimPosition(float planeY)
            => GetCrosshairPlanePosition(planeY);

        public Vector3 GetWorldAimPosition()
            => _worldAimPosition;

        public Vector2 GetCrosshairScreenPosition()
            => GetFinalCrosshairPixel();

        public Vector3 GetCrosshairPlanePosition()
            => GetCrosshairPlanePosition(transform.position.y);

        public Vector3 GetCrosshairPlanePosition(float planeY)
        {
            Ray aimRay = GetAimRay();
            Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

            if (aimPlane.Raycast(aimRay, out float enter))
                return aimRay.GetPoint(enter);

            return aimRay.GetPoint(fallbackAimDistance);
        }

        public Vector3 GetCrosshairWorldPosition()
        {
            Ray aimRay = GetAimRay();
            Camera mainCamera = Camera.main;

            if (Physics.Raycast(aimRay, out RaycastHit hit, mainCamera.farClipPlane, whatIsGround))
                return hit.point;

            return aimRay.GetPoint(fallbackAimDistance);
        }

        public float GetDistance()
        {
            Vector3 targetPosition = GetCrosshairWorldPosition();
            Vector3 position = transform.position;
            return Vector3.Distance(position, targetPosition);
        }

        private void HandleCursorMove(Vector2 delta)
        {
            if (!IsCursorLocked)
                return;

            Vector2 cursorDelta = delta * Sensitivity;
            ConsumeRecoilFromCursorDelta(ref cursorDelta);

            _userCursorPixel += cursorDelta;
            ClampToScreen(ref _userCursorPixel);
        }

        private void HandleChangeHandling(ChangeHandlingEvent eventData)
        {
            RefreshEquippedGunContext();
            UpdateSpreadRadiusPixels();
        }


        private void HandleGunAttack(GunAttackEvent eventData)
        {
            _currentGunData = eventData.GunData;
            ApplyShotRecoil(eventData);
            UpdateSpreadRadiusPixels();
        }


        public void MoveMouseToCursor()
        {
            if (Mouse.current != null)
                Mouse.current.WarpCursorPosition(GetFinalCrosshairPixel());
        }

        public void MoveCursorToMouse()
        {
            if (Mouse.current != null)
                _userCursorPixel = Mouse.current.position.ReadValue();

            _recoilTargetPixel = Vector2.zero;
            _recoilOffsetPixel = Vector2.zero;
            _recoilRecoveryVelocity = Vector2.zero;
        }

        private void RefreshEquippedGunContext()
        {
            _currentGunData = null;
            _currentGunObject = null;

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) &&
                item is GunItem gunItem)
            {
                _currentGunData = gunItem.GunItemData;
                _currentGunObject = gunItem.WeaponObj as GunObject;
            }

            UpdateCurrentCrosshairData(true);
        }

        private void UpdateCurrentCrosshairData(bool force = false)
        {
            CrosshairSO nextCrosshairData = ResolveCurrentCrosshairData();

            if (!force && CurrentCrosshairData == nextCrosshairData)
                return;

            CurrentCrosshairData = nextCrosshairData;
            _localEventBus.Raise(new CrosshairChangeEvent(_currentGunData, CurrentCrosshairData));
            Bus.Raise(new ChangeCameraZoom(CurrentCrosshairData != null
                ? CurrentCrosshairData.cameraFovReduction
                : 0f));
        }

        private CrosshairSO ResolveCurrentCrosshairData()
        {
            if (_currentGunData == null)
                return defaultCrosshair;

            if (_player.PlayerInput.AimKey && _currentGunData.aimCrosshairData != null)
                return _currentGunData.aimCrosshairData;

            return _currentGunData.crosshairData != null
                ? _currentGunData.crosshairData
                : defaultCrosshair;
        }

        private void ApplyShotRecoil(GunAttackEvent eventData)
        {
            _lastShotTime = Time.time;
            _recoilRecoveryVelocity = Vector2.zero;

            GunDataSO recoilData = eventData.GunData;
            float kickY = eventData.VerticalRecoil * recoilData.pixelsPerRecoilUnit;
            float kickX = eventData.HorizontalRecoil * recoilData.pixelsPerRecoilUnit;

            GetPlayerScreenAxes(out Vector2 rightAxis, out Vector2 forwardAxis);
            _recoilTargetPixel += rightAxis * kickX + forwardAxis * kickY;
        }

        private void RecoverRecoilTarget()
        {
            if (_currentGunData == null)
                return;

            if (_player.PlayerInput.AttackKey)
                return;

            if (Time.time - _lastShotTime < _currentGunData.recoilRecoveryStartTime)
                return;

            float recoveryTime = Mathf.Max(0.001f, _currentGunData.recoilRecoveryTime);
            _recoilTargetPixel = Vector2.SmoothDamp(
                _recoilTargetPixel,
                Vector2.zero,
                ref _recoilRecoveryVelocity,
                recoveryTime,
                _currentGunData.recoilRecovery,
                Time.deltaTime);

            if (_recoilTargetPixel.sqrMagnitude < 0.01f)
            {
                _recoilTargetPixel = Vector2.zero;
                _recoilRecoveryVelocity = Vector2.zero;
            }
        }

        private void UpdateRecoilOffset()
        {
            float followTime = Mathf.Max(0.001f, _currentGunData != null ? _currentGunData.recoilDuration : 0.05f);
            float followAlpha = 1f - Mathf.Exp(-Time.deltaTime / followTime);
            _recoilOffsetPixel = Vector2.Lerp(_recoilOffsetPixel, _recoilTargetPixel, followAlpha);

            if ((_recoilTargetPixel - _recoilOffsetPixel).sqrMagnitude < 0.01f)
                _recoilOffsetPixel = _recoilTargetPixel;
        }

        private void ConsumeRecoilFromCursorDelta(ref Vector2 cursorDelta)
        {
            ConsumeVisibleRecoilFromCursorDelta(ref cursorDelta);
            ConsumePendingRecoilFromCursorDelta(ref cursorDelta);
        }

        private void ConsumeVisibleRecoilFromCursorDelta(ref Vector2 cursorDelta)
        {
            if (_recoilOffsetPixel.sqrMagnitude < 0.0001f)
                return;

            Vector2 recoilAxis = _recoilOffsetPixel.normalized;
            float oppositeInput = Mathf.Max(0f, -Vector2.Dot(cursorDelta, recoilAxis));
            if (oppositeInput <= 0f)
                return;

            float consumedMagnitude = ConsumeMagnitudeAlongAxis(ref _recoilOffsetPixel, recoilAxis, oppositeInput);
            if (consumedMagnitude <= 0f)
                return;

            ConsumeMagnitudeAlongAxis(ref _recoilTargetPixel, recoilAxis, consumedMagnitude);
            cursorDelta += recoilAxis * consumedMagnitude;
        }

        private void ConsumePendingRecoilFromCursorDelta(ref Vector2 cursorDelta)
        {
            if (_recoilTargetPixel.sqrMagnitude < 0.0001f)
                return;

            Vector2 recoilAxis = _recoilTargetPixel.normalized;
            float oppositeInput = Mathf.Max(0f, -Vector2.Dot(cursorDelta, recoilAxis));
            if (oppositeInput <= 0f)
                return;

            float consumedMagnitude = ConsumeMagnitudeAlongAxis(ref _recoilTargetPixel, recoilAxis, oppositeInput);
            if (consumedMagnitude <= 0f)
                return;

            cursorDelta += recoilAxis * consumedMagnitude;
        }

        private static float ConsumeMagnitudeAlongAxis(ref Vector2 value, Vector2 axis, float maxMagnitude)
        {
            if (maxMagnitude <= 0f || value.sqrMagnitude < 0.0001f || axis.sqrMagnitude < 0.0001f)
                return 0f;

            axis.Normalize();
            float along = Vector2.Dot(value, axis);
            if (along <= 0f)
                return 0f;

            float consumed = Mathf.Min(maxMagnitude, along);
            value -= axis * consumed;

            if (value.sqrMagnitude < 0.01f)
                value = Vector2.zero;

            return consumed;
        }

        private void UpdateSpreadRadiusPixels()
        {
            CurrentSpreadRadiusPixels = 0f;

            PlayerStateEnum currentState = _player.StateMachine.CurrentStateEnum;
            if ((currentState != PlayerStateEnum.Idle && currentState != PlayerStateEnum.Walk &&
                 currentState != PlayerStateEnum.Aim && currentState != PlayerStateEnum.Attack &&
                 currentState != PlayerStateEnum.Reload) || _currentGunObject == null)
                return;

            Vector3 firePosition = _currentGunObject.FirePosition;
            Vector3 aimPointOnFirePlane = GetCrosshairPlanePosition(firePosition.y);
            Vector3 planarOffset = aimPointOnFirePlane - firePosition;
            planarOffset.y = 0f;

            float trajectoryDistance = planarOffset.magnitude;
            Vector3 fireDirection = _currentGunObject.FireDirection;
            float spreadAngleDeg = _currentGunObject.CurrentSpreadAngleDeg;

            Vector3 centerWorld = firePosition + fireDirection * trajectoryDistance;
            Vector3 leftWorld = firePosition +
                                Quaternion.AngleAxis(-spreadAngleDeg, Vector3.up) * fireDirection *
                                trajectoryDistance;
            Vector3 rightWorld = firePosition +
                                 Quaternion.AngleAxis(spreadAngleDeg, Vector3.up) * fireDirection *
                                 trajectoryDistance;

            Camera mainCamera = Camera.main;
            Vector3 centerScreen = mainCamera.WorldToScreenPoint(centerWorld);
            Vector3 leftScreen = mainCamera.WorldToScreenPoint(leftWorld);
            Vector3 rightScreen = mainCamera.WorldToScreenPoint(rightWorld);

            if (centerScreen.z <= 0f || leftScreen.z <= 0f || rightScreen.z <= 0f)
                return;

            Vector2 centerPixel = new Vector2(centerScreen.x, centerScreen.y);
            CurrentSpreadRadiusPixels = Mathf.Max(
                Vector2.Distance(centerPixel, new Vector2(leftScreen.x, leftScreen.y)),
                Vector2.Distance(centerPixel, new Vector2(rightScreen.x, rightScreen.y)));
        }

        private Ray GetAimRay()
        {
            return Camera.main.ScreenPointToRay(GetCrosshairScreenPosition());
        }

        private Vector2 GetFinalCrosshairPixel()
        {
            Vector2 finalAim = _userCursorPixel + _recoilOffsetPixel;
            ClampToScreen(ref finalAim);
            return finalAim;
        }

        private void GetPlayerScreenAxes(out Vector2 rightAxis, out Vector2 forwardAxis)
        {
            Camera cam = Camera.main;

            Vector3 from = cam.WorldToScreenPoint(_player.transform.position);
            Vector3 to = cam.WorldToScreenPoint(_player.transform.position + _player.transform.forward);

            Vector2 screenForward = new Vector2(to.x - from.x, to.y - from.y);
            if (screenForward.sqrMagnitude <= 0.0001f)
            {
                rightAxis = Vector2.right;
                forwardAxis = Vector2.up;
                return;
            }

            forwardAxis = screenForward.normalized;
            rightAxis = new Vector2(forwardAxis.y, -forwardAxis.x);
        }

        private void ClampToScreen(ref Vector2 p)
        {
            p.x = Mathf.Clamp(p.x, clampMargin, Screen.width - clampMargin);
            p.y = Mathf.Clamp(p.y, clampMargin, Screen.height - clampMargin);
        }

        public void SetSensitivity(float value)
            => _sensitivity = baseSensitivity * value;
    }
}