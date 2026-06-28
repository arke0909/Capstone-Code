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
    }
}