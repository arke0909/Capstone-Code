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
        private UIPanel _prevPanel;

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
        }

        protected override void OnDestroy()
        {
            foreach (var menu in _menus)
            {
                menu.MenuButton.onClick.RemoveAllListeners();
            }
            
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUI);
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

            if (!UIManager.Instance.TryGetCurrentPanel(out var panel))
                return;

            foreach (var menu in _menus)
            {
                if (menu.Panel == panel)
                {
                    SetMenuUI(menu, true);
                }
            }
        }

        private void ChangeUI(PlayerMenuUIButton playerMenuUI)
        {
            _prevPanel?.DisableUI();
            _prevPanel = playerMenuUI.Panel;
            
            SetMenuUI(playerMenuUI, true);
        }

        private void SetMenuUI(PlayerMenuUIButton playerMenuUI, bool isActive)
        {
            playerMenuUI.EnableUI();
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