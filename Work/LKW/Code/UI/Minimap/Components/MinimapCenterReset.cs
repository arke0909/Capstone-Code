using Code.UI.Minimap;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.UI.Minimap.Components
{
    public class MinimapCenterReset : MonoBehaviour
    {
        [SerializeField] private MinimapUI minimapUI;
        [SerializeField] private RectTransform targetRect;

        private void Awake()
        {
            if (targetRect == null && minimapUI != null)
                targetRect = minimapUI.MiniMapRect;
        }

        private void Update()
        {
            if (minimapUI == null || targetRect == null) return;
            if (!minimapUI.IsActive) return;
            if (Keyboard.current == null) return;
            if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

            targetRect.anchoredPosition = Vector2.zero;
        }
    }
}
