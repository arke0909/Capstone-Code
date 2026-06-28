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
public class MinimapSystem : MonoBehaviour
{
    [SerializeField] private Camera minimapCamera;

    [field:SerializeField] public RectTransform MinimapRect { get; private set; }
    public Dictionary<string, MinimapElementData> AllData { get; private set; } = new Dictionary<string, MinimapElementData>();

    public event Action<MinimapElementData> OnDataAdded;
    public event Action<string> OnDataRemoved;


    private void Awake()
    {
        Debug.Assert(MinimapRect != null, "MinimapRect is null");
        Debug.Assert(minimapCamera != null, "MinimapCamera is null");
    }

    private void OnEnable()
    {
        Bus.Subscribe<AddMinimapElementEvent>(HandleAdd);
        Bus.Subscribe<RemoveMinimapElementEvent>(HandleRemove);
    }

    private void OnDisable()
    {
        Bus.Unsubscribe<AddMinimapElementEvent>(HandleAdd);
        Bus.Unsubscribe<RemoveMinimapElementEvent>(HandleRemove);
    }

    private void HandleAdd(AddMinimapElementEvent evt)
    {
        evt.ElementData.NormalizedPos = WorldToNormalizedPosition(evt.WorldInitPos);
        if (AllData.ContainsKey(evt.ElementData.Id)) return;

        AllData.Add(evt.ElementData.Id, evt.ElementData);
        OnDataAdded?.Invoke(evt.ElementData);
    }

    private void HandleRemove(RemoveMinimapElementEvent evt)
    {
        string targetId = evt.ID;
        
        if (string.IsNullOrEmpty(targetId)) return;
        if (AllData.TryGetValue(targetId, out var elementData))
        {
            AllData.Remove(targetId);
            OnDataRemoved?.Invoke(elementData.Id);
        }
    }

    public Vector2 WorldToNormalizedPosition(Vector3 worldPos)
    {
        Vector3 camPos = minimapCamera.transform.position;
        float h = minimapCamera.orthographicSize;
        float w = h * minimapCamera.aspect;

        float nx = Mathf.InverseLerp(camPos.x - w, camPos.x + w, worldPos.x);
        float ny = Mathf.InverseLerp(camPos.z - h, camPos.z + h, worldPos.z);

        return new Vector2(nx, ny);
    }
    
    public Vector3 MinimapToWorldPosition(Vector2 anchoredPos)
    {
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
