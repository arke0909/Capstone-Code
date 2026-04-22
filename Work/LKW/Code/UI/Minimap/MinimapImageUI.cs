using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Code.UI.Minimap
{
    public class MinimapImageUI : MonoBehaviour,
        IPointerDownHandler,
        IDragHandler
    {
        [SerializeField] private RectTransform parentRect;

        private RectTransform _rectTransform;
        private Vector2 _moveOffset;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            _moveOffset = _rectTransform.anchoredPosition - localPoint;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            Debug.Log("DDD");

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

            _rectTransform.anchoredPosition = localPoint + _moveOffset;
        }
    }
}