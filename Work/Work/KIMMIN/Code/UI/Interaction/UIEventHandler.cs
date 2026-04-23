using System;
using System.Collections.Generic;
using Code.UI.Core.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using Work.Code.UI.Core.Interaction;
using Work.Code.UI.Interaction;

namespace Code.UI.Core
{
    public enum EUIEvent
    {
        PointerEnter,
        PointerExit,
        PointerClick,
        RightClick,
        DragBegin,
        Drag,
        DragEnd,
        Drop,
        None
    }
    
    [DefaultExecutionOrder(-15)]
    public class UIEventHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
    {
        private IClickable _clickable;
        private IHoverable _hoverable;
        private IDraggable _draggable;
        private IDroppable _droppable;
        
        public Dictionary<EUIEvent, Action<PointerEventData>> EventHandler { get; private set; }
        
        private void Awake()
        {
            EventHandler = new();
            
            _clickable = GetComponent<IClickable>();
            _hoverable = GetComponent<IHoverable>();
            _draggable = GetComponent<IDraggable>();
            _droppable = GetComponent<IDroppable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(EventHandler.TryGetValue(EUIEvent.PointerEnter, out var evt))
                evt?.Invoke(eventData);
            
            _hoverable?.OnHoverEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(EventHandler.TryGetValue(EUIEvent.PointerExit, out var evt))
                evt?.Invoke(eventData);
            
            _hoverable?.OnHoverExit(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(EventHandler.TryGetValue(EUIEvent.PointerClick, out var evt))
                evt?.Invoke(eventData);
            
            _clickable?.OnClick(eventData);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (EventHandler.TryGetValue(EUIEvent.RightClick, out var rightEvt))
                    rightEvt?.Invoke(eventData);
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {            
            if(EventHandler.TryGetValue(EUIEvent.DragBegin, out var evt)) 
                evt?.Invoke(eventData);
            
            _draggable?.OnDragStart(eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if(EventHandler.TryGetValue(EUIEvent.Drag, out var evt))
                evt?.Invoke(eventData);
            
            _draggable?.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(EventHandler.TryGetValue(EUIEvent.DragEnd, out var evt))
                evt?.Invoke(eventData);
            
            _draggable?.OnDragEnd(eventData);
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            if(EventHandler.TryGetValue(EUIEvent.Drop, out var evt))
                evt?.Invoke(eventData);

            _droppable?.OnDrop(eventData);
        }
        
        public void BindUIEvent(InteractableUI owner, Action<PointerEventData> action, EUIEvent type = EUIEvent.PointerClick)
        {
            UIEventHandler handler = owner.EventHandler;

            if (handler.EventHandler.ContainsKey(type))
            {
                handler.EventHandler[type] -= action;
                handler.EventHandler[type] += action;
            }
            else if (!handler.EventHandler.ContainsKey(type))
                handler.EventHandler[type] = action;
        }
        
        public void UnBindUIEvent(InteractableUI owner, Action<PointerEventData> action, EUIEvent type = EUIEvent.PointerClick)
        {
            UIEventHandler handler = owner.EventHandler;

            if (handler.EventHandler.TryGetValue(type, out var existingAction))
            {
                existingAction -= action;

                if (existingAction == null)
                    handler.EventHandler.Remove(type);
                else
                    handler.EventHandler[type] = existingAction;
            }
        }

        public void ClearUIEvent(InteractableUI owner, EUIEvent type = EUIEvent.PointerClick)
        {
            UIEventHandler handler = owner.EventHandler;
            handler.EventHandler.Remove(type);
        }
        
        public void ClearAll() => EventHandler?.Clear();
    }
}   