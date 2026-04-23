using System;
using Code.UI.Core;
using Code.UI.Popup;
using UnityEngine;
using Work.Code.UI.ContextMenu;

namespace Work.Code.UI.Core.Interaction
{
    [RequireComponent(typeof(UIEventHandler))]
    public class InteractableUI : UIBase
    {
        public UIEventHandler EventHandler { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            EventHandler = GetComponent<UIEventHandler>();
        }

        protected override void OnDestroy()
        {
            ClearInteractEvents();
            base.OnDestroy();
        }

        protected void BindTooltip<T>(Func<T> data, float duration = 0f)
        {
            UIOverlayManager.Instance?.BindTooltip(this, data, duration);
        }

        protected void UnbindTooltip()
        {
            UIOverlayManager.Instance?.UnbindTooltip(this);
        }
        
        protected void BindContextMneu<T>(ContextMenuSO menu, Func<T> data)
        {
            UIOverlayManager.Instance?.BindContextMenu(this, menu, data);
        }

        protected void UnBindContextMneu()
        {
            UIOverlayManager.Instance?.UnbindContextMenu(this);
        }
        
        protected void BindPopup(IPopupable popupable)
        {
            UIOverlayManager.Instance?.BindPopup(popupable);
        }
        
        protected void UnBindPopup(IPopupable popupable)
        {
            UIOverlayManager.Instance?.UnbindPopup(popupable);
        }

        protected virtual void ClearInteractEvents() { }
    }
}