using UnityEngine.EventSystems;

namespace Work.Code.UI.Interaction
{
    public interface IDroppable
    {
        void OnDrop(PointerEventData eventData);
    }
}