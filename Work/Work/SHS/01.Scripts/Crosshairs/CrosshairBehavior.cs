using Chipmunk.ComponentContainers;
using UnityEngine;
using UnityEngine.InputSystem;
using Scripts.Combat.Datas;
using Scripts.Combat.ItemObjects;
using Scripts.Players;
using InGame.PlayerUI;
using Code.ETC;
using Code.Players;
using Work.LKW.Code.Items;
using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.InventorySystems.Equipments;
using DewmoLib.Dependencies;
using Scripts.Players.States;
using SHS.Scripts.Combats.Events;
using Work.SHS.Items.Events;
using Random = UnityEngine.Random;

namespace SHS.Scripts.Crosshairs
{
    [Provide]
    public class CrosshairBehavior : MonoBehaviour, IContainerComponent, IAfterInitialze, IAimProvider,
        IDependencyProvider
    {
        [Header("Input")] [SerializeField] private float sensitivity = 18f;
        [SerializeField] private float clampMargin = 8f;

        [Header("Aim Sampling")] [SerializeField]
        private LayerMask whatIsGround;

        [SerializeField] private float minAimDistance = 1f;
        [SerializeField] private float fallbackAimDistance = 100f;

        [Header("Spread")] [SerializeField] private float defaultSpreadTargetDistance = 20f;

        public ComponentContainer ComponentContainer { get; set; }
        public bool IsCursorLocked { get; private set; } = true;
        public float CurrentSpreadRadiusPixels { get; private set; }

        private Player _player;
        private PlayerEquipment _equipment;
        private LocalEventBus _localEventBus;

        private GunDataSO _currentGunData;
        private GunObject _currentGunObject;

        private Vector2 _userAimPx;
        private Vector2 _recoilTargetPx;
        private Vector2 _recoilCurrentPx;
        private Vector2 _recoilVelocityPx;
        private Vector3 _aimPosition;
        private float _lastShotTime = -999f;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;

            _player = componentContainer.Get<Player>(true);
            _equipment = componentContainer.Get<PlayerEquipment>();
            _localEventBus = componentContainer.Get<LocalEventBus>();

            _localEventBus.Subscribe<ItemEquippedEvent>(HandleItemEquipped);
            _localEventBus.Subscribe<ItemUnEquippedEvent>(HandleItemUnEquipped);
            _localEventBus.Subscribe<GunAttackEvent>(HandleGunAttack);

            EventBus.Subscribe<ChangeCursorEvent>(HandleChangeCursor);

            _userAimPx = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        public void AfterInitialize()
        {
            RefreshCurrentGunContext();
            UpdateSpreadRadiusPixels();
        }

        private void OnDestroy()
        {
            _localEventBus.Unsubscribe<ItemEquippedEvent>(HandleItemEquipped);
            _localEventBus.Unsubscribe<ItemUnEquippedEvent>(HandleItemUnEquipped);
            _localEventBus.Unsubscribe<GunAttackEvent>(HandleGunAttack);

            EventBus.Unsubscribe<ChangeCursorEvent>(HandleChangeCursor);
        }

        private void Update()
        {
            if (IsCursorLocked)
            {
                Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                _userAimPx += mouseDelta * sensitivity;
                ClampToScreen(ref _userAimPx);

                TickRecoilRecovery();
                TickRecoilFollow();
            }

            UpdateSpreadRadiusPixels();
        }

        private void LateUpdate()
        {
            Vector3 newAimPosition = GetCrosshairPlanePosition();
            if (Vector3.Distance(transform.position, newAimPosition) < minAimDistance)
                return;
            _aimPosition = newAimPosition;
        }

        public Vector3 GetAimPosition()
            => _aimPosition;

        public Vector2 GetCrosshairScreenPosition()
        {
            return GetFinalAimPx();
        }

        public Vector3 GetCrosshairPlanePosition()
        {
            Ray aimRay = GetAimRay();
            Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

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
        
        private void HandleChangeCursor(ChangeCursorEvent evt)
        {
            IsCursorLocked = evt.IsLocked;

            if (IsCursorLocked)
            {
                if (Mouse.current != null)
                    _userAimPx = Mouse.current.position.ReadValue();

                _recoilTargetPx = Vector2.zero;
                _recoilCurrentPx = Vector2.zero;
                _recoilVelocityPx = Vector2.zero;
            }
            else
            {
                if (Mouse.current != null)
                    Mouse.current.WarpCursorPosition(GetFinalAimPx());
            }
        }

        private void HandleItemEquipped(ItemEquippedEvent eventData)
        {
            if (eventData.EquipableItem?.EquipItemData is GunDataSO)
            {
                RefreshCurrentGunContext();
                UpdateSpreadRadiusPixels();
            }
        }

        private void HandleItemUnEquipped(ItemUnEquippedEvent eventData)
        {
            if (eventData.EquipableItem?.EquipItemData is GunDataSO)
            {
                RefreshCurrentGunContext();
                UpdateSpreadRadiusPixels();
            }
        }

        private void HandleGunAttack(GunAttackEvent eventData)
        {
            _currentGunData = eventData.GunData;
            ApplyShotRecoil(eventData.GunData);
            UpdateSpreadRadiusPixels();
        }

        private void RefreshCurrentGunContext()
        {
            _currentGunData = null;
            _currentGunObject = null;

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) && item is GunItem gunItem)
            {
                _currentGunData = gunItem.GunItemData;
                _currentGunObject = gunItem.WeaponObj as GunObject;
            }

            GunDataSO gunData = _currentGunData != null ? _currentGunData : null;
            _localEventBus.Raise(new CrosshairChangeEvent(gunData));
        }

        private void ApplyShotRecoil(GunDataSO recoilData)
        {
            _lastShotTime = Time.time;

            float verticalMultiplier = Random.Range(recoilData.minVerticalMultiplier, recoilData.maxVerticalMultiplier);
            float horizontalMultiplier =
                Random.Range(recoilData.minHorizontalMultiplier, recoilData.maxHorizontalMultiplier);

            float kickY = recoilData.verticalRecoil * verticalMultiplier * recoilData.pixelsPerRecoilUnit;
            float kickX = recoilData.horizontalRecoil * horizontalMultiplier * recoilData.pixelsPerRecoilUnit;

            GetPlayerScreenAxes(out Vector2 rightAxis, out Vector2 forwardAxis);
            _recoilTargetPx += rightAxis * kickX + forwardAxis * kickY;
        }

        private void TickRecoilFollow()
        {
            float smoothTime = Mathf.Max(0.001f, _currentGunData != null ? _currentGunData.recoilDuration : 0.05f);
            _recoilCurrentPx = Vector2.SmoothDamp(
                _recoilCurrentPx,
                _recoilTargetPx,
                ref _recoilVelocityPx,
                smoothTime,
                Mathf.Infinity,
                Time.deltaTime
            );
        }

        private void TickRecoilRecovery()
        {
            if (_currentGunData == null)
                return;

            if (Time.time - _lastShotTime < _currentGunData.recoilRecoveryStartTime)
                return;

            float recoveryTime = Mathf.Max(0.001f, _currentGunData.recoilRecoveryTime);
            float rate = _currentGunData.recoilRecovery / recoveryTime;
            float alpha = 1f - Mathf.Exp(-rate * Time.deltaTime);

            _recoilTargetPx = Vector2.Lerp(_recoilTargetPx, Vector2.zero, alpha);

            if (_recoilTargetPx.sqrMagnitude < 0.01f)
                _recoilTargetPx = Vector2.zero;
        }

        private void UpdateSpreadRadiusPixels()
        {
            float spreadAngleDeg = 0f;
            float spreadTargetDistance = defaultSpreadTargetDistance;

            PlayerStateEnum currentState = _player.StateMachine.CurrentStateEnum;
            if ((currentState == PlayerStateEnum.Aim || currentState == PlayerStateEnum.Attack ||
                 currentState == PlayerStateEnum.Reload) && _currentGunObject != null)
            {
                spreadAngleDeg = _currentGunObject.CurrentSpreadAngleDeg;

                Vector3 firePosition = _currentGunObject.FirePosition;
                Vector3 aimPointOnFirePlane = GetCrosshairPlanePosition();
                Vector3 planarOffset = aimPointOnFirePlane - firePosition;
                planarOffset.y = 0f;
                spreadTargetDistance = planarOffset.magnitude;
            }

            CurrentSpreadRadiusPixels = CalculateSpreadRadiusPixels(spreadAngleDeg, spreadTargetDistance);
        }

        private float CalculateSpreadRadiusPixels(float spreadAngleDeg, float targetDistance)
        {
            Camera mainCamera = Camera.main;

            float distance = Mathf.Max(0.01f, targetDistance);
            float clampedAngle = Mathf.Clamp(spreadAngleDeg, 0f, 89f);
            float worldRadius = Mathf.Tan(clampedAngle * Mathf.Deg2Rad) * distance;

            Vector3 centerWorld = GetCrosshairWorldPosition();
            Vector3 edgeWorld = centerWorld + mainCamera.transform.right * worldRadius;

            Vector3 centerScreen = mainCamera.WorldToScreenPoint(centerWorld);
            Vector3 edgeScreen = mainCamera.WorldToScreenPoint(edgeWorld);

            if (centerScreen.z <= 0f || edgeScreen.z <= 0f)
                return 0f;

            return Vector2.Distance(
                new Vector2(centerScreen.x, centerScreen.y),
                new Vector2(edgeScreen.x, edgeScreen.y));
        }

        private Ray GetAimRay()
        {
            return Camera.main.ScreenPointToRay(GetCrosshairScreenPosition());
        }

        private Vector2 GetFinalAimPx()
        {
            Vector2 finalAim = _userAimPx + _recoilCurrentPx;
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
    }
}