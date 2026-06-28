using Code.UI.Minimap;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.UI.Minimap.Components
{
public class MinimapZoom : MonoBehaviour
{
    private const float SliderMaxValue = 100f;
    
    [SerializeField] private MinimapUI minimapUI;
    [SerializeField] private Slider slider;

    [Header("Zoom")]
    [SerializeField] private float maxZoomInSize = 1000f;
    [SerializeField] private float maxZoomOutSize = 400f;
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minElementScale = 0.75f;
    [SerializeField] private float maxElementScale = 1.75f;

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void Update()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) <= 0) return;

        float size = Mathf.Clamp(
            minimapUI.MiniMapRect.sizeDelta.x + scroll * zoomSpeed,
            maxZoomOutSize, maxZoomInSize
        );
        ApplyZoom(size);

        float t = (size - maxZoomOutSize) / (maxZoomInSize - maxZoomOutSize);
        slider.SetValueWithoutNotify(t * SliderMaxValue);
    }

    private void OnSliderChanged(float v)
    {
        float t = v / SliderMaxValue;
        float size = Mathf.Lerp(maxZoomOutSize, maxZoomInSize, t);
        ApplyZoom(size);
    }

    private void ApplyZoom(float size)
    {
        minimapUI.MiniMapRect.sizeDelta = new Vector2(size, size);

        float t = (size - maxZoomOutSize) / (maxZoomInSize - maxZoomOutSize);
        minimapUI.SetSize(Mathf.Lerp(minElementScale, maxElementScale, t));
        minimapUI.UpdateElementsPosition();
    }
}
}
