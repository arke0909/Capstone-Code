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

        private MenuButtonUI[] _menus;
        private UIPanel _prevPanel;

        protected override void Awake()
        {
            base.Awake();
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUI);
            
            _menus = GetComponentsInChildren<MenuButtonUI>();
            foreach (var menu in _menus)
            {
                menu.MenuButton.onClick.AddListener(() => ChangeUI(menu));
            }
            
            DisableUI();
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUI);

            foreach (var menu in _menus)
            {
                menu.MenuButton.onClick.RemoveAllListeners();
            }
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

            if (!UIManager.Instance.GetCurrentPanel(out var panel)) return;

            foreach (var menu in _menus)
            {
                if(menu.Panel == panel)
                    SetMenuUI(menu, true);
            }
        }

        private void ChangeUI(MenuButtonUI menu)
        {
            _prevPanel?.DisableUI();
            _prevPanel = menu.Panel;
            menu.Panel.EnableUI();
            
            SetMenuUI(menu, true);
        }

        private void SetMenuUI(MenuButtonUI menu, bool isActive)
        {
            DisableHighlight();
            menu.SetHighlight(isActive);
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