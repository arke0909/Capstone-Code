using Chipmunk.GameEvents;
using Code.GameEvents;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class ReloadTutorialState : TutorialState
    {
        public override void EnterTutorial()
        {
            base.EnterTutorial();
            EventBus.Subscribe<AmmoUpdateEvent>(HandleChangeAmmo);
        }

        private void HandleChangeAmmo(AmmoUpdateEvent evt)
        {
            if (evt.CurrentAmmo == evt.TotalAmmo)
            {
                TutorialComplete();
            }
        }

        public override void ExitTutorial()
        {
            EventBus.Unsubscribe<AmmoUpdateEvent>(HandleChangeAmmo);
        }
    }
}