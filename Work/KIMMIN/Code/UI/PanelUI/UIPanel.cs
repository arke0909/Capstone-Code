using Chipmunk.GameEvents;
using Code.GameEvents;
using UnityEngine;

namespace Code.UI.Core
{
    public class UIPanel : UIBase
    {
        [SerializeField] private string panelID;

        public string PanelID => panelID;
        public sealed override EUILayer Layer => EUILayer.Panel;


        protected override void Awake()
        {
            base.Awake();
            DisableUI();
        }

        public override void ToggleUI(bool isFade = false)
        {
            base.ToggleUI(isFade);
            EventBus.Raise(new PlayerUIEvent(IsActive));
        }
    }
}
