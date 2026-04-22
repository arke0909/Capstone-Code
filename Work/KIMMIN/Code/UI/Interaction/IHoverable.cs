using UnityEngine.EventSystems;

namespace Code.UI.Core.Interaction
{
    public interface IHoverable
    {
        public void OnHoverEnter(PointerEventData eventData);
        public void OnHoverExit(PointerEventData eventData);
    }
}