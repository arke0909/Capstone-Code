using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.PlayerUI.Menu;
using Work.Code.UI.Core.Interaction;

namespace Work.Code.UI
{
    [RequireComponent(typeof(Button))]
    public class PlayerMenuUIButton : InteractableUI
    {
        [SerializeField] private AlertController alertController;
        
        private Image _icon;
        private readonly Color32 _highlightColor = new(50, 150, 200, 255);
        
        [field: SerializeField] public UIPanel Panel { get; private set; }
        [field: SerializeField] public Button MenuButton { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            _icon = GetComponent<Image>();
        }

        public void SetHighlight(bool isActive)
        {
            _icon.color = isActive ? _highlightColor : Color.white;

            if (isActive)
                alertController.SetAlert(false);
        }
    }
}