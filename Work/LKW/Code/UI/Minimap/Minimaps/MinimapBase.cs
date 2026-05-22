using System;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Code.UI.Minimap.Core;
using Code.UI.Minimap.Factory;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;

namespace Code.UI.Minimap.Minimaps
{
    public abstract class MinimapBase : UIBase
    {
        [Inject] protected Player _player;

        [SerializeField] protected MinimapSystem minimapSystem;
        [SerializeField] protected RectTransform miniMapRect;
        [SerializeField] protected RectTransform playerDot;

        private Dictionary<ElementType, MinimapFactory> _factories;
        private Dictionary<string, MinimapElement> _elements = new Dictionary<string, MinimapElement>();

        protected virtual void OnEnable()
        {
            _factories = GetComponentsInChildren<MinimapFactory>()
                .ToDictionary(factory => factory.Type, factory => factory);
            
            minimapSystem.OnDataAdded += HandleUIAdded;
            minimapSystem.OnDataRemoved += HandleUIRemoved;

        }

        protected virtual void OnDisable()
        {
            minimapSystem.OnDataAdded -= HandleUIAdded;
            minimapSystem.OnDataRemoved -= HandleUIRemoved;
        }

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
            _elements[id].DestroySelf();
            _elements.Remove(id);
            
        }
        
        protected void UpdateElementsPosition()
        {
            foreach (var element in _elements.Values)
            {
                element.Rect.anchoredPosition = new Vector2(
                    (element.NormalizedPos.x - 0.5f) * miniMapRect.sizeDelta.x,
                    (element.NormalizedPos.y - 0.5f) * miniMapRect.sizeDelta.y
                );
            }
        }

        // 백분율로 넘겨주면됨
        protected void SetSize(float scaleFactor)
        {
            foreach (var element in _elements.Values)
            {
                if (element.SyncChildScale)
                {
                    element.Rect.sizeDelta = new Vector2(element.OriginSize.x * scaleFactor, element.OriginSize.y * scaleFactor);
                }
            }
        }
        
        

        protected void UpdatePlayerDot()
        {
            if (_player == null || minimapSystem == null) return;

            Vector2 normalizedPos = minimapSystem.WorldToNormalizedPosition(_player.transform.position);

            playerDot.anchoredPosition = new Vector2(
                (normalizedPos.x - 0.5f) * miniMapRect.sizeDelta.x,
                (normalizedPos.y - 0.5f) * miniMapRect.sizeDelta.y
            );

            float playerRotation = _player.transform.eulerAngles.y;
            playerDot.localRotation = Quaternion.Euler(0, 0, -playerRotation);
        }
    }
}