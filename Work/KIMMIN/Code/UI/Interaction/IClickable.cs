using UnityEngine.EventSystems;

namespace Code.UI.Core.Interaction
{
    public delegate void OnClickEvent(IClickable clickable);
    
    public interface IClickable
    {
        public void OnClick(PointerEventData eventData) { }
    }
}