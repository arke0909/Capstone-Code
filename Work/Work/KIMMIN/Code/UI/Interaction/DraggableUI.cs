using System;
using Chipmunk.GameEvents;
using Code.UI.Core.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using Work.Code.SkillInventory.GameEvents;
using Work.Code.UI.Core.Interaction;

namespace Work.Code.UI.Interaction
{
    public class DraggableUI : InteractableUI, IDraggable
    {
        public event Action<DraggableUI> OnDragStartEvent;
        public event Action<PointerEventData> OnDragEvent;
        public event Action OnDragEndEvent;
        
        public virtual Sprite DragSprite { get; set; }
        
        public void OnDragStart(PointerEventData eventData)
        {
            if (!CanDrag() || eventData.button != PointerEventData.InputButton.Left) return;
            OnDragStartEvent?.Invoke(this);
            HandleDragStart(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            HandleDrag(eventData);
            OnDragEvent?.Invoke(eventData);
        }

        public void OnDragEnd(PointerEventData eventData)
        {
            HandleDragEnd(eventData);
            OnDragEndEvent?.Invoke();
        }
        
        protected virtual void HandleDragStart(PointerEventData eventData)
        {
            EventBus.Raise(new DragEvent(DragSprite, true));
        }
        
        protected virtual void HandleDrag(PointerEventData eventData) { }

        protected virtual void HandleDragEnd(PointerEventData eventData)
        {
            EventBus.Raise(new DragEvent(null, false));
        }

        public virtual bool CanDrag() => true;

        protected override void ClearInteractEvents()
        {
            base.ClearInteractEvents();
            OnDragStartEvent = null;
            OnDragEvent = null;
            OnDragEndEvent = null;
        }
    }
}