using System;
using System.Collections.Generic;
using Code.UI.Controller;
using Code.UI.Popup;
using UnityEngine;
using Work.Code.Core;
using Work.Code.UI.ContextMenu;
using Work.Code.UI.Core.Interaction;

namespace Code.UI.Core
{
    public class UIOverlayInfo
    {
        public bool hasTooltip;
        public bool hasContextMenu;
        public bool hasPopup;
    }
    
    public class OverlayUIManager : MonoSingleton<OverlayUIManager>
    {
        [SerializeField] private TooltipController tooltipController;
        [SerializeField] private PopupController popupController;
        [SerializeField] private ContextMenuController contextMenuController;

        private Dictionary<InteractableUI, UIOverlayInfo> _overlayInfos = new();
        
        public void BindTooltip<T>(InteractableUI owner, Func<T> data, float duration = 0f)
        {
            tooltipController.BindTooltip(owner, data, duration);
            GetOverlayInfo(owner).hasTooltip = true;
        }

        public void UnbindTooltip(InteractableUI owner)
        {
            tooltipController.UnbindTooltip(owner);
            GetOverlayInfo(owner).hasTooltip = false;
        }
        
        public void BindPopup(IPopupable popupable)
        {
            if (popupable is not InteractableUI interactable) 
                return;
            
            popupController.BindPopup(popupable);
            GetOverlayInfo(interactable).hasPopup = true;
        }
        
        public void UnbindPopup(IPopupable popupable)
        {
            if (popupable is not InteractableUI interactable) 
                return;
            
            popupController.UnbindPopup(popupable);
            GetOverlayInfo(interactable).hasPopup = false;
        }
        
        public void BindContextMenu<T>(InteractableUI owner, ContextMenuSO menu, Func<T> data)
        {
            contextMenuController.BindContextMenu(owner, menu, data);
            GetOverlayInfo(owner).hasContextMenu = true;
        }
        
        public void UnbindContextMenu(InteractableUI owner)
        {
            contextMenuController.UnbindContextMenu(owner);
            GetOverlayInfo(owner).hasContextMenu = false;
        }
        
        public UIOverlayInfo GetOverlayInfo(InteractableUI owner)
        {
            if (!_overlayInfos.TryGetValue(owner, out var overlayInfo))
            {
                overlayInfo = new UIOverlayInfo();
                _overlayInfos.Add(owner, overlayInfo);
            }

            return overlayInfo;
        }

        public bool HasActiveOverlay()
        {
            return _overlayInfos.Count > 0;
        }

        public void CloseAllOverlays()
        {
            foreach (var ui in _overlayInfos.Values)
            {
                
            }
        }
        
        public void ClearOverlay(InteractableUI owner)
        {
            if (!_overlayInfos.TryGetValue(owner, out var state)) return;

            if (state.hasTooltip)
                tooltipController.UnbindTooltip(owner);
            if (state.hasContextMenu)
                contextMenuController.UnbindContextMenu(owner);
            if (state.hasPopup && owner is IPopupable popupable)
                popupController.UnbindPopup(popupable);

            _overlayInfos.Remove(owner);
        }
    }
}