using Chipmunk.GameEvents;
using Code.GameEvents;

namespace Code.UI.Core
{
    public abstract class UIPanel : UIBase
    {
        public override EUILayer Layer => EUILayer.Panel;

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