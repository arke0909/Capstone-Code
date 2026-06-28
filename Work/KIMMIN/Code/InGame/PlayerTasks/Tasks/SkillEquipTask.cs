using Chipmunk.ComponentContainers;
using Scripts.Players;
using Scripts.SkillSystem.Manage;
using UnityEngine;
using Work.Code.Tutorials;

namespace Work.Code.PlayerTasks
{
    public class SkillEquipTask : PlayerTask
    {
        [SerializeField] private TutorialDoor tutorialDoor;
        private SkillManager _skillManager;
        private ActiveSkillComponent _activeComponent;
        private PassiveSkillComponent _passiveComponent;
        
        private const string ReloadTaskText = "스킬을 드래그 해 장착하세요.";

        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            _skillManager = _player.Get<SkillManager>();
            _activeComponent = _player.Get<ActiveSkillComponent>();
            _passiveComponent = _player.Get<PassiveSkillComponent>();
        }

        public override void StartTask()
        {
            base.StartTask();

            if (_activeComponent.HasAnySkill() || _passiveComponent.HasAnySkill())
            {
                CompleteTask();
                return;
            }
            
            _skillManager.OnSkillEquip += HandleEquipSkill;
        }
        
        private void HandleEquipSkill()
        {
            CompleteTask();
        }


        protected override void StopTask()
        {
            _skillManager.OnSkillEquip -= HandleEquipSkill;
        }

        protected override string GetTaskText()
        {
            return ReloadTaskText;
        }
    }
}