using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using UnityEngine;
using Chipmunk.Library.Utility.GameEvents.Local;
using DewmoLib.Dependencies;
using InGame.PlayerUI;
using Scripts.Combat.Datas;

namespace SHS.Scripts.Crosshairs
{
    public class CrosshairManager : MonoBehaviour
    {
        [Header("Crosshair Assets")] [SerializeField]
        private CrosshairSO defaultCrosshair;

        [SerializeField] private CrosshairSO[] crosshairDatas;

        private readonly Dictionary<CrosshairSO, VirtualCrosshair> _crosshairs = new();

        public VirtualCrosshair CurrentCrosshair { get; private set; }

        [Inject] private CrosshairBehavior _crosshairBehavior;
        private LocalEventBus _localEventBus;

        public void Start()
        {
            _localEventBus = _crosshairBehavior.Get<LocalEventBus>();
            _localEventBus.Subscribe<CrosshairChangeEvent>(HandleCrosshairChange);
            EventBus.Subscribe<ChangeCursorEvent>(HandleCursorStateChange);

            RegisterCrosshair(defaultCrosshair);

            for (int i = 0; i < crosshairDatas.Length; i++)
                RegisterCrosshair(crosshairDatas[i]);

            SetCurrentCrosshair(defaultCrosshair);
        }


        private void OnDestroy()
        {
            _localEventBus.Unsubscribe<CrosshairChangeEvent>(HandleCrosshairChange);
            EventBus.Unsubscribe<ChangeCursorEvent>(HandleCursorStateChange);
        }

        private void LateUpdate()
        {
            Vector2 screenPosition = _crosshairBehavior.GetCrosshairScreenPosition();
            CurrentCrosshair.SetScreenPosition(screenPosition);
            CurrentCrosshair.SetSpreadRadiusPixels(_crosshairBehavior.CurrentSpreadRadiusPixels);
            CurrentCrosshair.SetVisible(_crosshairBehavior.IsCursorLocked);
            CurrentCrosshair.SetRangeText(_crosshairBehavior.GetDistance());
        }

        private void HandleCursorStateChange(ChangeCursorEvent evt)
        {
            if (evt.IsLocked)
                _crosshairBehavior.MoveCursorToMouse();
            else
                _crosshairBehavior.MoveMouseToCursor();
        }

        private void HandleCrosshairChange(CrosshairChangeEvent eventData)
        {
            GunDataSO gunData = eventData.GunData;
            CrosshairSO targetData = gunData != null && gunData.crosshairData != null
                ? gunData.crosshairData
                : defaultCrosshair;
            SetCurrentCrosshair(targetData, eventData.GunData);
        }

        private void SetCurrentCrosshair(CrosshairSO crosshairData, GunDataSO gunData = null)
        {
            RegisterCrosshair(crosshairData);

            VirtualCrosshair targetCrosshair = _crosshairs[crosshairData];
            targetCrosshair?.SetGunData(gunData);

            if (CurrentCrosshair == targetCrosshair)
                return;

            if (CurrentCrosshair != null)
                CurrentCrosshair.gameObject.SetActive(false);

            CurrentCrosshair = targetCrosshair;
            CurrentCrosshair.gameObject.SetActive(true);
        }

        private void RegisterCrosshair(CrosshairSO crosshairData)
        {
            if (crosshairData == null || _crosshairs.ContainsKey(crosshairData))
                return;

            Transform parent = transform;
            GameObject crosshairObject = Instantiate(crosshairData.crosshairPrefab, parent);
            VirtualCrosshair crosshair = crosshairObject.GetComponent<VirtualCrosshair>();

            crosshair.gameObject.SetActive(false);
            _crosshairs.Add(crosshairData, crosshair);
        }
    }
}