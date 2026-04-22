using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;
using Scripts.SkillSystem.Skills;
using System;
using Scripts.SkillSystem;

namespace Scripts.Players.States
{
    public class PlayerSkillState : PlayerState
    {
        private ActiveSkillComponent _skillCompo;
        private ActiveSkill _currentSkill;
        private IUseStateSkill _stateSkill;
        private static readonly int _skillHash = Animator.StringToHash("SkillIndex");
        public PlayerSkillState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _skillCompo = container.Get<ActiveSkillComponent>(true);
        }
        public override void Enter()
        {
            base.Enter();
            _movement.StopImmediately();
            Debug.Assert(_skillCompo != null && _skillCompo.CurrentSkill != null && _skillCompo.CurrentSkill is IUseStateSkill,
                "CurrentSkill is null but you are in skill state");
            _currentSkill = _skillCompo.CurrentSkill;
            _stateSkill = _currentSkill as IUseStateSkill;
            _animator.SetParam(_skillHash, (int)_stateSkill.AnimType);
            _animatorTrigger.OnCastSkillTrigger += HandleSkillCast;
            _currentSkill.StartAndUseSkill();
        }

        private void HandleSkillCast()
        {
            _stateSkill.OnSkillTrigger();
        }

        public override void Update()
        {
            base.Update();
            if (_isTriggerCall)
                _player.ChangeState(PlayerStateEnum.Idle);
        }
        public override void Exit()
        {
            base.Exit();
            _currentSkill.EndSkill();
            _animatorTrigger.OnCastSkillTrigger -= HandleSkillCast;
        }
    }
}
