using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Code.UI.Core;
using Code.UI.Minimap.Core;
using Code.UI.Minimap.Factory;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine.Serialization;

namespace Code.UI.Minimap
{
    public class MinimapUI : UIPanel
    {
        [Inject] private MinimapSystem _minimapSystem;
        [Inject] private Player _player;
        
        [Header("Zoom Settings")]
        [SerializeField] private float maxZoomInSize = 1000f;
        [SerializeField] private float maxZoomOutSize = 400f;
        [SerializeField] private float zoomSpeed = 20f;

        [Header("UI Components")]
        [SerializeField] private RectTransform miniMapRect;
        [SerializeField] private RectTransform playerDot;
        [SerializeField] private Slider slider;

        private Dictionary<ElementType, MinimapFactory> _factories;
        private Dictionary<string, MinimapElement> _elements = new Dictionary<string, MinimapElement>();
        
        
        protected void OnEnable()
        {
            _factories = GetComponentsInChildren<MinimapFactory>()
                .ToDictionary(factory => factory.Type, factory => factory);
            
            _player.PlayerInput.OnMinimapPressed += HandleMinimapPressed;
            slider.onValueChanged.AddListener(SetSliderValue);
            _minimapSystem.OnDataAdded += HandleUIAdded;
            _minimapSystem.OnDataRemoved += HandleUIRemoved;
        }

        protected override void OnDestroy()
        {
            if (_player != null)
                _player.PlayerInput.OnMinimapPressed -= HandleMinimapPressed;
            _minimapSystem.OnDataAdded -= HandleUIAdded;
            _minimapSystem.OnDataRemoved -= HandleUIRemoved;
            base.OnDestroy();
        }
        
        private void Update()
        {
            HandleZoom();
            UpdatePlayerDot();
        }
        
        #region  Handler
        
        private void HandleUIAdded(MinimapElementData data)
        {
             if(_factories.ContainsKey(data.Type) == false) return;
             _elements.Add(data.Id, _factories[data.Type].CreateUIElement(data));
             _elements[data.Id].transform.SetParent(miniMapRect);
             UpdateElementsPosition();
        }
        
        private void HandleUIRemoved(string id)
        {
            if(string.IsNullOrEmpty(id)) return;
            _elements.Remove(id);
        }

        private void HandleMinimapPressed()
        {
            _minimapSystem.IsActiveMinimap = !_minimapSystem.IsActiveMinimap;
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

       

        private void UpdateElementsPosition()
        {
            foreach (var element in _elements.Values)
            {
                element.Rect.anchoredPosition = new Vector2(
                    (element.NormalizedPos.x - 0.5f) * miniMapRect.sizeDelta.x,
                    (element.NormalizedPos.y - 0.5f) * miniMapRect.sizeDelta.y
                );
            }
        }

        private void UpdatePlayerDot()
        {
            if (_player == null || _minimapSystem == null) return;

            Vector2 normalizedPos = _minimapSystem.WorldToNormalizedPosition(_player.transform.position);

            playerDot.anchoredPosition = new Vector2(
                (normalizedPos.x - 0.5f) * miniMapRect.sizeDelta.x,
                (normalizedPos.y - 0.5f) * miniMapRect.sizeDelta.y
            );

            float playerRotation = _player.transform.eulerAngles.y;
            playerDot.localRotation = Quaternion.Euler(0, 0, -playerRotation);
        }

        private void SetSliderValue(float v)
        {
            float size = Mathf.Lerp(maxZoomOutSize, maxZoomInSize, v / 100f);
            miniMapRect.sizeDelta = new Vector2(size, size);
            
            float scaleFactor = v / 100f + 1;
            
            foreach (var element in _elements.Values)
            {
                if (element.SyncChildScale)
                {
                    element.Rect.sizeDelta = new Vector2(element.OriginSize.x * scaleFactor, element.OriginSize.y * scaleFactor);
                }
            }
            
            UpdateElementsPosition();
        }
    }
}