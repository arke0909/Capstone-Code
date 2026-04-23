using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.Events;
using Code.UI.Minimap.Core;
using DewmoLib.Dependencies;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.UI.Minimap
{
    public class MinimapSystem : MonoBehaviour, IDependencyProvider
    {
        [field:SerializeField] public RectTransform MinimapRect { get; set; }
        [SerializeField] private Camera minimapCamera;

        [Provide]
        public MinimapSystem GetSystem() => this;
        
        public Dictionary<string, MinimapElementData> AllData { get; private set; } = new Dictionary<string, MinimapElementData>();

        public event Action<MinimapElementData> OnDataAdded;
        public event Action<string> OnDataRemoved;

        public bool IsActiveMinimap { get; set; } = false;

        private void OnEnable()
        {
            Bus.Subscribe<AddMinimapElementEvent>(HandleAdd);
            Bus.Subscribe<RemoveMinimapElementEvent>(HandleRemove);
        }

        private void HandleAdd(AddMinimapElementEvent evt)
        {
            evt.ElementData.NormalizedPos = WorldToNormalizedPosition(evt.WorldInitPos);
            OnDataAdded?.Invoke(evt.ElementData);
            AllData.Add(evt.ElementData.Id, evt.ElementData);
        }

        private void HandleRemove(RemoveMinimapElementEvent evt)
        {
            string targetId = evt.ID;
            
            if (string.IsNullOrEmpty(targetId)) return;
          
            if (AllData.TryGetValue(targetId, out var elementData))
            {
                OnDataRemoved?.Invoke(elementData.Id);
                AllData.Remove(targetId);
            }
        }

        public Vector2 WorldToNormalizedPosition(Vector3 worldPos)
        {
            if (minimapCamera == null) return Vector2.one * 0.5f; // 기본값 중앙

            Vector3 camPos = minimapCamera.transform.position;
            float h = minimapCamera.orthographicSize;
            float w = h * minimapCamera.aspect;
    
            float nx = Mathf.InverseLerp(camPos.x - w, camPos.x + w, worldPos.x);
            float ny = Mathf.InverseLerp(camPos.z - h, camPos.z + h, worldPos.z);
    
            return new Vector2(nx, ny);
        }
        
        public Vector3 MinimapToWorldPosition(Vector2 anchoredPos)
        {
            if (minimapCamera == null) return Vector3.zero;

            Vector3 camPos = minimapCamera.transform.position;
            float h = minimapCamera.orthographicSize;
            float w = h * minimapCamera.aspect;

            float nx = (anchoredPos.x / MinimapRect.rect.width) + 0.5f;
            float ny = (anchoredPos.y / MinimapRect.rect.height) + 0.5f;

            float worldX = Mathf.Lerp(camPos.x - w, camPos.x + w, nx);
            float worldZ = Mathf.Lerp(camPos.z - h, camPos.z + h, ny);

            return new Vector3(worldX, 0f, worldZ); 
        }
        
        public bool IsPointInMinimapRect()
        {
            if (!MinimapRect.gameObject.activeInHierarchy) return false;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    MinimapRect, 
                    Mouse.current.position.ReadValue(), 
                    null, 
                    out Vector2 localPoint))
            {
                return MinimapRect.rect.Contains(localPoint);
            }

            return false;
        }
    }
}