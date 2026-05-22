using Chipmunk.ComponentContainers;
using Scripts.Players;
using Scripts.SkillSystem.Manage;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class SkillEquipTutorialState : TutorialState
    {
        [SerializeField] private TutorialDoor tutorialDoor;
        
        private SkillManager _skillManager;
        private ActiveSkillComponent _activeComponent;
        private PassiveSkillComponent _passiveComponent;
        
        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            
            _skillManager = _player.Get<SkillManager>();
            _activeComponent = _player.Get<ActiveSkillComponent>();
            _passiveComponent = _player.Get<PassiveSkillComponent>();
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            
            if(_activeComponent.HasAnySkill() || _passiveComponent.HasAnySkill())
                TutorialComplete();
            
            _skillManager.OnSkillEquip += HandleEquipSkill;
        }

        private void HandleEquipSkill()
        {
            TutorialComplete();
        }

        public override void ExitTutorial()
        {
            tutorialDoor.OpenDoor();
            _skillManager.OnSkillEquip -= HandleEquipSkill;
        }
    }
}