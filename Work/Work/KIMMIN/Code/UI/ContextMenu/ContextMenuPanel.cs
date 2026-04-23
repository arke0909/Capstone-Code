using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Work.Code.UI.ContextMenu
{
    [RequireComponent(typeof(Button))]
    public class ContextMenuPanel : UIBase
    {
        [field: SerializeField] public Button PanelButton { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            DisableUI();
        }
    }
}