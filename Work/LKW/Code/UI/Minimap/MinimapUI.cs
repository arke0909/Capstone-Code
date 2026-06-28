using System;
using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Code.UI.Minimap.Core;
using Code.UI.Minimap.Factory;
using UnityEngine;

namespace Code.UI.Minimap
{
public class MinimapUI : UIBase
{
    public sealed override EUILayer Layer => uiLayer;
    [SerializeField] private EUILayer uiLayer;
    [field: SerializeField] public RectTransform MiniMapRect { get; private set; }
    [field: SerializeField] public MinimapSystem MinimapSystem { get; private set; }

    [SerializeField] private bool showOnStart;

    private Dictionary<ElementType, MinimapFactory> _factories;
    private readonly Dictionary<string, MinimapElement> _elements = new();
    private float _currentElementScale = 1f;

    private void Start()
    {
        if (showOnStart) ShowUIOnInspector();
        else
            DisableUI();
    }

    protected void OnEnable()
    {
        _factories = GetComponentsInChildren<MinimapFactory>()
            .ToDictionary(f => f.Type, f => f);

        MinimapSystem.OnDataAdded += HandleUIAdded;
        MinimapSystem.OnDataRemoved += HandleUIRemoved;
    }

    protected void OnDisable()
    {
        MinimapSystem.OnDataAdded -= HandleUIAdded;
        MinimapSystem.OnDataRemoved -= HandleUIRemoved;
    }

    private void HandleUIAdded(MinimapElementData data)
    {
        if (!_factories.TryGetValue(data.Type, out var factory)) return;
        if (_elements.ContainsKey(data.Id)) return;

        MinimapElement element = factory.CreateUIElement(data);
        if (element == null) return;

        _elements.Add(data.Id, element);
        element.transform.SetParent(MiniMapRect, false);
        ApplyElementScale(element);
        UpdateElementsPosition();
    }
    
    private void HandleUIRemoved(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!_elements.TryGetValue(id, out var element)) return;

        element.ReturnPool();
        _elements.Remove(id);
    }

    public void UpdateElementsPosition()
    {
        foreach (var element in _elements.Values)
        {
            element.Rect.anchoredPosition = new Vector2(
                (element.NormalizedPos.x - 0.5f) * MiniMapRect.sizeDelta.x,
                (element.NormalizedPos.y - 0.5f) * MiniMapRect.sizeDelta.y
            );
        }
    }

    public void SetSize(float scaleFactor)
    {
        _currentElementScale = scaleFactor;

        foreach (var element in _elements.Values)
        {
            ApplyElementScale(element);
        }
    }

    private void ApplyElementScale(MinimapElement element)
    {
        element.Rect.localScale = element.SyncChildScale
            ? element.OriginScale * _currentElementScale
            : element.OriginScale;
    }
}
}
