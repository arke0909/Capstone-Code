using System;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Code.UI.Minimap.Core;
using Code.UI.Minimap.Factory;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.UI.Minimap.FixedMinimap
{
    public class FixedMinimapUI : UIPanel
    {
        [Inject] private MinimapSystem _minimapSystem;
        [Inject] private Player _player;
        
        [Header("UI Components")]
        [SerializeField] private RectTransform miniMapRect;
        [SerializeField] private RectTransform playerDot;
        
        private Dictionary<ElementType, MinimapFactory> _factories;
        private Dictionary<string, MinimapElement> _elements = new Dictionary<string, MinimapElement>();

        private void Start()
        {
            ShowUIOnInspector();
        }

        protected  void OnEnable()
        {
            _factories = GetComponentsInChildren<MinimapFactory>()
                .ToDictionary(factory => factory.Type, factory => factory);
            
            _minimapSystem.OnDataAdded += HandleDataAdded;
            _minimapSystem.OnDataRemoved += HandleDataRemoved;
        }

        public override void ToggleUI(bool isFade = false)
        {
        }

        private void Update()
        {
            UpdatePlayerDot();
            SetMapImageOffset();
        }

        #region  Handler
        
        private void HandleDataAdded(MinimapElementData data)
        {
            if(_factories.ContainsKey(data.Type) == false) return;

             _elements.Add(data.Id, _factories[data.Type].CreateUIElement(data));
             _elements[data.Id].transform.SetParent(miniMapRect);
             UpdateElementsPosition();
        }
        
        private void HandleDataRemoved(string id)
        {
            if(string.IsNullOrEmpty(id)) return;
            _elements.Remove(id);
        }
        
        private void HandleMinimapPressed() => ToggleUI(true);
        
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
            Vector2 playerPos = _minimapSystem.WorldToNormalizedPosition(_player.transform.position);
            playerDot.anchoredPosition = new Vector2(
                (playerPos.x - 0.5f) * miniMapRect.rect.width,
                (playerPos.y - 0.5f) * miniMapRect.rect.height
            );
            
            Quaternion playerRot = _player.transform.rotation;
            
            Quaternion dotRot = Quaternion.Euler(new Vector3(0,0, -playerRot.eulerAngles.y));
            
            playerDot.rotation = dotRot;
        }

        private void SetMapImageOffset()
        {
            Vector2 followPos = -playerDot.anchoredPosition;
            miniMapRect.anchoredPosition = followPos;
        }
    }
}