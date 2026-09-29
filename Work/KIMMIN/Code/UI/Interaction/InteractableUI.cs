using System;
using Code.UI.Core;
using UnityEngine;
using Work.Code.UI.ContextMenu;

namespace Work.Code.UI.Core.Interaction
{
    [RequireComponent(typeof(UIEventHandler))]
    public class InteractableUI : UIBase
    {
        public UIEventHandler EventHandler => GetComponent<UIEventHandler>();

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnDestroy()
        {
            ClearInteractEvents();
            base.OnDestroy();
        }

        protected void BindTooltip<T>(Func<T> data, float duration = 0f)
        {
            OverlayUIManager.Instance?.BindTooltip(this, data, duration);
        }

        protected void UnbindTooltip()
        {
            OverlayUIManager.Instance?.UnbindTooltip(this);
        }
        
        protected void BindContextMenu<T>(ContextMenuSO menu, Func<T> data)
        {
            OverlayUIManager.Instance?.BindContextMenu(this, menu, data);
        }

        protected void UnBindContextMenu()
        {
            OverlayUIManager.Instance?.UnbindContextMenu(this);
        }
        protected virtual void ClearInteractEvents() { }
    }
}
