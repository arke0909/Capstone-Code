using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Tooltip
{
    [DefaultExecutionOrder(-9)]
    public class TooltipMover : MonoBehaviour
    {
        [SerializeField] private Vector2 offset = new Vector2(5f, 5f);
        [SerializeField] private Canvas canvas;

        private RectTransform _canvasRect;
        private RectTransform _tooltipRoot;
        private VerticalLayoutGroup _layout;
        private Vector3 _prevMousePos;

        public void InitMover(RectTransform tooltipRoot)
        {
            _tooltipRoot = tooltipRoot;
            _layout = _tooltipRoot.GetComponent<VerticalLayoutGroup>();
        }

        private void Awake()
        {
            _canvasRect = canvas.transform as RectTransform;
        }

        private void LateUpdate()
        {
            if (_tooltipRoot == null || _tooltipRoot.childCount == 0) return;
            if (Input.mousePosition != _prevMousePos)
            {
                SetPosition();
                _prevMousePos = Input.mousePosition;
            }
        }

        private void SetPosition()
        {
            RectTransform parent = transform as RectTransform;
            RectTransform root = _tooltipRoot;
            RectTransform canvasRect = _canvasRect;
            Vector2 mousePos = Input.mousePosition;

            Vector2 localPos;
            Vector2 dir;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, 
                mousePos, canvas.worldCamera, out localPos);

            Vector2 center = new Vector2(
                (canvasRect.rect.xMin + canvasRect.rect.xMax) * 0.5f,
                (canvasRect.rect.yMin + canvasRect.rect.yMax) * 0.5f
            );

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
            parent.anchoredPosition = localPos;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, root);
            Vector3 pos = parent.anchoredPosition;

            float minX = -canvasRect.rect.width * canvasRect.pivot.x;
            float maxX = canvasRect.rect.width * (1 - canvasRect.pivot.x);
            float minY = -canvasRect.rect.height * canvasRect.pivot.y;
            float maxY = canvasRect.rect.height * (1 - canvasRect.pivot.y);

            if (bounds.min.x < minX)
                pos.x += (minX - bounds.min.x);
            if (bounds.max.x > maxX)
                pos.x -= (bounds.max.x - maxX);
            if (bounds.min.y < minY)
                pos.y += (minY - bounds.min.y);
            if (bounds.max.y > maxY)
                pos.y -= (bounds.max.y - maxY);

            parent.anchoredPosition = pos;
        }
    }
}