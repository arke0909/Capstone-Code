using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI;

namespace InGame.PlayerUI
{
    public class PlayerMenuUI : UIBase
    { 
        [SerializeField] private Image indicatorUI;

        private PlayerMenuUIButton[] _menus;
        private UIPanel _currentPanel;

        protected override void Awake()
        {
            base.Awake();
            DisableUI();

            _menus = GetComponentsInChildren<PlayerMenuUIButton>();
            
            foreach (var menu in _menus)
            {
                menu.MenuButton.onClick.AddListener(() => ChangeUI(menu));
            }
            
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUI);

            if (UIManager.HasInstance)
                UIManager.Instance.OnUIStackChanged += HandleUIStackChanged;
        }

        protected override void OnDestroy()
        {
            foreach (var menu in _menus)
            {
                menu.MenuButton.onClick.RemoveAllListeners();
            }
            
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUI);

            if (UIManager.HasInstance)
                UIManager.Instance.OnUIStackChanged -= HandleUIStackChanged;
        }
        
        private void HandlePlayerUI(PlayerUIEvent evt)
        {
            if(evt.IsEnabled)
                EnableUI(true);
            else
                DisableUI(true);
        }

        public override void EnableUI(bool isFade = false)
        {
            base.EnableUI(isFade);
            HandleUIStackChanged();
        }

        private void HandleUIStackChanged()
        {
            if (!UIManager.Instance.TryGetCurrentPanel(out var panel))
            {
                _currentPanel = null;
                DisableHighlight();
                return;
            }

            foreach (var menu in _menus)
            {
                if (menu.Panel == panel)
                {
                    _currentPanel = panel;
                    SetMenuUI(menu, true);
                    return;
                }
            }
        }

        private void ChangeUI(PlayerMenuUIButton playerMenuUI)
        {
            bool changed;
            if (UIManager.HasInstance)
                changed = UIManager.Instance.ReplaceStackUI(_currentPanel, playerMenuUI.Panel, true);
            else
            {
                _currentPanel?.DisableUI();
                playerMenuUI.Panel.EnableUI(true);
                changed = true;
            }

            if (!changed)
                return;

            _currentPanel = playerMenuUI.Panel;
            SetMenuUI(playerMenuUI, true);
        }

        private void SetMenuUI(PlayerMenuUIButton playerMenuUI, bool isActive)
        {
            DisableHighlight();
            playerMenuUI.SetHighlight(isActive);
        }

        private void DisableHighlight()
        {
            foreach (var menu in _menus)
            {
                menu.SetHighlight(false);
            }
        }
    }
}
