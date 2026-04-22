using UnityEngine.EventSystems;

namespace Code.UI.Core.Interaction
{
    public interface IDraggable
    {
        void OnDragStart(PointerEventData eventData);
        void OnDrag(PointerEventData eventData) { }
        void OnDragEnd(PointerEventData eventData);
    }
}