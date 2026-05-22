using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Tutorials
{
    public class PinTutorialUI : UIPanel
    {
        [SerializeField] private Button button;

        protected override void Awake()
        {
            base.Awake();
            button.onClick.AddListener(HandleClicked);
            DisableUI();
        }

        private void HandleClicked()
        {
            DisableUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            button.onClick.RemoveListener(HandleClicked);
        }
    }
}