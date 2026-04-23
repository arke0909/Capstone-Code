using Scripts.SkillSystem.Manage;
using Scripts.SkillSystem.Skills;
using Chipmunk.ComponentContainers;
using System;

namespace Scripts.Players.States
{
    public class PlayerSkillAimState : PlayerMoveState
    {
        private ActiveSkillComponent _skillCompo;
        private IAimSkill _aimSkill;
        public PlayerSkillAimState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _skillCompo = container.Get<ActiveSkillComponent>();
        }
        public override void Enter()
        {
            base.Enter();
            _aimSkill = _skillCompo.CurrentSkill.GetComponent<IAimSkill>();
            _aimSkill.StartAiming();
            _player.PlayerInput.OnSkillPressed += HandleSkillPressed;
        }
        public override void Update()
        {
            base.Update();
            if (_player.PlayerInput.AttackKey)
                _player.ChangeState(PlayerStateEnum.Skill);
        }
        public override void Exit()
        {
            base.Exit();
            _player.PlayerInput.OnSkillPressed -= HandleSkillPressed;
        }
        private void HandleSkillPressed(ActiveSlotType type)
        {
            if (_skillCompo.CurrentSkillIndex == type)
            {
                _aimSkill.CancelSkill();
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }
    }
}
