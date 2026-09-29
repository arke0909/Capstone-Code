using System;
using Code.UI.Controller;
using Code.UI.Popup;
using UnityEngine;
using Work.Code.Core;
using Work.Code.UI.ContextMenu;
using Work.Code.UI.Core.Interaction;

namespace Code.UI.Core
{
    public class OverlayUIManager : MonoSingleton<OverlayUIManager>
    {
        [SerializeField] private TooltipController tooltipController;
        [SerializeField] private PopupController popupController;
        [SerializeField] private ContextMenuController contextMenuController;
        protected override bool PersistAcrossScenes => false;

        public void BindTooltip<T>(InteractableUI owner, Func<T> data, float delay = 0f)
        {
            tooltipController.BindTooltip(owner, data, delay);
        }

        public void UnbindTooltip(InteractableUI owner)
        {
            tooltipController.UnbindTooltip(owner);
        }
        
        public void ShowPopup<T>(T data, ICallbackData callback = null)
        {
            popupController.ShowPopup(data, callback);
        }

        public void HidePopup()
        {
            popupController.HidePopup();
        }
        
        public void BindContextMenu<T>(InteractableUI owner, ContextMenuSO menu, Func<T> data)
        {
            contextMenuController.BindContextMenu(owner, menu, data);
        }
        
        public void UnbindContextMenu(InteractableUI owner)
        {
            contextMenuController.UnbindContextMenu(owner);
        }

        public bool HasActiveOverlay()
        {
            return tooltipController.HasActiveTooltip()
                   || popupController.HasActivePopup()
                   || contextMenuController.HasActiveMenu();
        }

        public void CloseAllOverlays()
        {
            tooltipController.HideAll();
            popupController.HideAllPopups();
            contextMenuController.HideCurrentMenu();
        }
    }
}
