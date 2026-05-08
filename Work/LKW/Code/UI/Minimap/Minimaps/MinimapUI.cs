using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Code.UI.Minimap.Core;
using Code.UI.Minimap.Factory;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.UI.Minimap.Minimaps
{
    public class MinimapUI : MinimapBase
    {
        [Header("Zoom Settings")]
        [SerializeField] private float maxZoomInSize = 1000f;
        [SerializeField] private float maxZoomOutSize = 400f;
        [SerializeField] private float zoomSpeed = 20f;

        [Header("UI Components")]
        [SerializeField] private Slider slider;

        
        protected override void OnEnable()
        {
            base.OnEnable();
            _player.PlayerInput.OnMinimapPressed += HandleMinimapPressed;
            slider.onValueChanged.AddListener(SetSliderValue);
        }

        protected override void OnDestroy()
        {
            if (_player != null)
                _player.PlayerInput.OnMinimapPressed -= HandleMinimapPressed;
           
            base.OnDestroy();
        }
        
        private void Update()
        {
            HandleZoom();
            UpdatePlayerDot();
        }
        
        #region  Handler
        private void HandleMinimapPressed()
        {
            minimapSystem.IsActiveMinimap = !minimapSystem.IsActiveMinimap;
            ToggleUI(true);
        }

        private void HandleZoom()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) <= 0) return;

            float size = Mathf.Clamp(miniMapRect.sizeDelta.x + scroll * zoomSpeed, maxZoomOutSize, maxZoomInSize);
            miniMapRect.sizeDelta = new Vector2(size, size);
            
            slider.value = (1 - (maxZoomInSize - size) / (maxZoomInSize - maxZoomOutSize)) * 100f;
        }
        #endregion

        private void SetSliderValue(float v)
        {
            float size = Mathf.Lerp(maxZoomOutSize, maxZoomInSize, v / 100f);
            miniMapRect.sizeDelta = new Vector2(size, size);
            
            float scaleFactor = v / 100f + 1;
            
           SetSize(scaleFactor);
            
            UpdateElementsPosition();
        }
    }
}