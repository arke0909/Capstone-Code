using Chipmunk.ComponentContainers;
using Scripts.Players;
using Scripts.SkillSystem.Manage;

namespace Work.Code.Tutorials
{
    public class SkillUseTutorialState : TutorialState
    {
        private ActiveSkillComponent _activeComponent;
        private PassiveSkillComponent _passiveComponent;
        
        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            
            _activeComponent = _player.Get<ActiveSkillComponent>();
            _passiveComponent = _player.Get<PassiveSkillComponent>();
        }

        public override void EnterTutorial()
        {
            TutorialComplete();
        }
    }
}