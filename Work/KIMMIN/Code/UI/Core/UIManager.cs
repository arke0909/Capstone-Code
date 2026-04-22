using System;
using System.Collections.Generic;
using Chipmunk.GameEvents;
using Code.GameEvents;
using DG.Tweening;
using UnityEngine;
using Work.Code.Core;
using Work.Code.GameEvents;

namespace Code.UI.Core
{
    public enum EUILayer
    {
        HUD,
        Panel,
        Popup,
        ContextMenu,
        Tooltip,
        None
    }
    
    [DefaultExecutionOrder(-25)]
    public class UIManager : MonoSingleton<UIManager>
    { 
        [SerializeField] private PlayerInputSO playerInput;

        private readonly HashSet<UIBase> _registeredUIs = new();
        private readonly Stack<UIBase> _uiStack = new();
        
        public UIOverlayManager OverlayManager => UIOverlayManager.Instance;
        public event Action OnUIStackChanged;

        private void Awake()
        {
            playerInput.OnToggleUIPressed += HandlePressEsc;
        }

        private void OnDestroy()
        {
            foreach (var ui in _registeredUIs)
            {
                ui.OnToggleUI -= HandleChangeUIState;
            }
            
            playerInput.OnToggleUIPressed -= HandlePressEsc;
        }
        
        public void RegisterUI(UIBase ui)
        {
            if (!_registeredUIs.Add(ui)) return;
            ui.OnToggleUI += HandleChangeUIState;
        }

        public void UnRegisterUI(UIBase ui)
        {
            if (!_registeredUIs.Contains(ui)) return;
            _registeredUIs.Remove(ui);
            ui.OnToggleUI -= HandleChangeUIState;
        }
        
        private void HandleChangeUIState(UIBase ui, bool isFade)
        {
            HandleToggle(ui, ui.IsActive, isFade);
            HandleStack(ui, ui.IsActive);
        }

        private void HandleStack(UIBase ui, bool isActive)
        {
            if (!CanStack(ui)) return;

            if (isActive)
                PushUI(ui);
            else
                PopUI();
            
            OnUIStackChanged?.Invoke();
            playerInput.SetPlayerInput(_uiStack.Count == 0);
        }

        private bool CanStack(UIBase ui)
        {
            return ui.Layer == EUILayer.Panel || ui.Layer == EUILayer.Popup;
        }

        private void HandlePressEsc()
        {
            if (OverlayManager.HasActiveOverlay())
                OverlayManager.CloseAllOverlays();
            
            if (_uiStack.Count == 0)
            {
                EventBus.Raise(new PressESCEvent());
                return;
            }
            
            EventBus.Raise(new PlayerUIEvent(false));
            PopUI();
        }

        public void PushUI(UIBase ui)
        {
            if (_uiStack.Contains(ui)) return;

            if (ui.Layer == EUILayer.Panel)
                ClearStack();

            _uiStack.Push(ui);
        }

        private void ClearStack()
        {
            while (_uiStack.Count > 0)
            {
                var top = _uiStack.Pop();
                top.DisableUI();
            }
        }
        
        public void PopUI()
        {
            if (_uiStack.Count == 0) return;
            var top = _uiStack.Pop();
            top.DisableUI();
        }

        public bool GetCurrentPanel(out UIPanel panel)
        {
            panel = null;
            
            foreach (var ui in _uiStack)
            {
                if (ui.Layer == EUILayer.Panel)
                {
                    panel = ui as UIPanel;
                    return true;
                }
            }

            return false;
        }
        
        public bool HasStackUI()
        {
            return _uiStack.Count > 0;
        }
        
        private void HandleToggle(UIBase ui, bool isActive, bool useFade)
        {
            var cg = ui.CanvasGroup;
            cg.DOKill(true);

            if (useFade)
            {
                if (isActive) {
                    cg.alpha = 0;
                    ToggleCanvasGroup(cg, true);
                    cg.DOFade(1, 0.1f).SetUpdate(true);
                }
                else {
                    cg.DOFade(0, 0.1f).OnComplete(() => {
                        ToggleCanvasGroup(cg, false);
                    }).SetUpdate(true);
                }
            }
            else {
                cg.alpha = isActive ? 1 : 0;
                ToggleCanvasGroup(cg, isActive);
            }
        }

        private void ToggleCanvasGroup(CanvasGroup cg, bool isActive)
        {
            cg.interactable = isActive;
            cg.blocksRaycasts = isActive;
        }
    }
}