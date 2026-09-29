using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI.Minimap.Components
{
    public class MinimapDrag : MonoBehaviour, IDragHandler
    {
        [SerializeField] private bool useDragLimit = true;
        [SerializeField] private bool useDynamicLimitByViewport = true;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private Vector2 dragLimitFromCenter = new Vector2(300f, 300f);

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private Vector2 _originPosition;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _originPosition = _rectTransform.anchoredPosition;

            if (viewportRect == null)
                viewportRect = _rectTransform.parent as RectTransform;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float scaleFactor = _canvas != null ? _canvas.scaleFactor : 1f;
            Vector2 nextPosition = _rectTransform.anchoredPosition + eventData.delta / scaleFactor;
            _rectTransform.anchoredPosition = ClampPosition(nextPosition);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || _rectTransform == null) return;
            _rectTransform.anchoredPosition = ClampPosition(_rectTransform.anchoredPosition);
        }

        private Vector2 ClampPosition(Vector2 position)
        {
            if (!useDragLimit)
                return position;

            Vector2 limit = GetDragLimit();

            position.x = Mathf.Clamp(
                position.x,
                _originPosition.x - limit.x,
                _originPosition.x + limit.x);

            position.y = Mathf.Clamp(
                position.y,
                _originPosition.y - limit.y,
                _originPosition.y + limit.y);

            return position;
        }

        private Vector2 GetDragLimit()
        {
            if (!useDynamicLimitByViewport || viewportRect == null)
                return dragLimitFromCenter;

            Vector2 contentSize = _rectTransform.rect.size;
            Vector2 viewportSize = viewportRect.rect.size;

            return new Vector2(
                Mathf.Max(0f, (contentSize.x - viewportSize.x) * 0.5f),
                Mathf.Max(0f, (contentSize.y - viewportSize.y) * 0.5f)
            );
        }
    }
}
