using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.UI.Tooltip
{
    [DefaultExecutionOrder(-9)]
    public class TooltipMover : MonoBehaviour
    {
        [SerializeField] private Vector2 offset = new Vector2(5f, 5f);
        [SerializeField] private Canvas canvas;

        private RectTransform _tooltipRoot;
        private RectTransform _canvasRect;
        private RectTransform _parent;
        private VerticalLayoutGroup _layout;
        private Vector2 _prevMousePos;

        public void InitMover(RectTransform tooltipRoot)
        {
            _tooltipRoot = tooltipRoot;
            _parent = transform as RectTransform;
            _layout = _tooltipRoot.GetComponent<VerticalLayoutGroup>();
        }

        private void Awake()
        {
            _canvasRect = canvas.transform as RectTransform;
        }

        private void LateUpdate()
        {
            if (_tooltipRoot == null || _tooltipRoot.childCount == 0) return;

            Vector2 mousePosition = GetMousePosition();
            if (mousePosition != _prevMousePos)
            {
                SetPosition(mousePosition);
                _prevMousePos = mousePosition;
            }
        }

        public void InvalidatePosition()
        {
            _prevMousePos = Vector2.positiveInfinity;
        }

        private Vector2 GetMousePosition()
        {
            return Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : Input.mousePosition;
        }

        private void SetPosition(Vector2 mousePos)
        {
            Vector2 localPos;
            Vector2 dir;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, 
                mousePos, canvas.worldCamera, out localPos);

            Vector2 screenMin;
            Vector2 screenMax;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, Vector2.zero, canvas.worldCamera, out screenMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, new Vector2(Screen.width, Screen.height), canvas.worldCamera, out screenMax);

            Vector2 center = (screenMin + screenMax) * 0.5f;

            if (localPos.x > center.x && localPos.y > center.y) {
                _layout.childAlignment = TextAnchor.LowerRight;
                dir = new Vector2(-1, -1);
            }
            else if (localPos.x < center.x && localPos.y > center.y) {
                _layout.childAlignment = TextAnchor.LowerLeft;
                dir = new Vector2(1, -1);
            }
            else if (localPos.x < center.x && localPos.y < center.y) {
                _layout.childAlignment = TextAnchor.UpperLeft;
                dir = new Vector2(1, 1);
            }
            else {
                _layout.childAlignment = TextAnchor.UpperRight;
                dir = new Vector2(-1, 1);
            }

            localPos += new Vector2(offset.x * dir.x, offset.y * dir.y);
            _parent.anchoredPosition = localPos;

            if (!TryGetVisibleTooltipBounds(out Bounds bounds))
                return;
            
            Vector3 pos = _parent.anchoredPosition;

            float minX = -_canvasRect.rect.width * _canvasRect.pivot.x;
            float maxX = _canvasRect.rect.width * (1 - _canvasRect.pivot.x);
            float minY = -_canvasRect.rect.height * _canvasRect.pivot.y;
            float maxY = _canvasRect.rect.height * (1 - _canvasRect.pivot.y);

            if (bounds.min.x < minX)
                pos.x += (minX - bounds.min.x);
            if (bounds.max.x > maxX)
                pos.x -= (bounds.max.x - maxX);
            if (bounds.min.y < minY)
                pos.y += (minY - bounds.min.y);
            if (bounds.max.y > maxY)
                pos.y -= (bounds.max.y - maxY);

            _parent.anchoredPosition = pos;
        }

        private bool TryGetVisibleTooltipBounds(out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;

            for (int i = 0; i < _tooltipRoot.childCount; i++)
            {
                Transform child = _tooltipRoot.GetChild(i);

                if (!IsVisibleTooltip(child, out RectTransform rect))
                    continue;

                Bounds childBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_canvasRect, rect);

                if (!hasBounds)
                {
                    bounds = childBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(childBounds);
                }
            }

            return hasBounds;
        }

        private bool IsVisibleTooltip(Transform child, out RectTransform rect)
        {
            rect = child as RectTransform;

            if (rect == null || !child.gameObject.activeInHierarchy)
                return false;

            if (child.TryGetComponent(out LayoutElement layoutElement) && layoutElement.ignoreLayout)
                return false;

            if (child.TryGetComponent(out CanvasGroup canvasGroup) && canvasGroup.alpha <= 0f)
                return false;

            return true;
        }
    }
}
