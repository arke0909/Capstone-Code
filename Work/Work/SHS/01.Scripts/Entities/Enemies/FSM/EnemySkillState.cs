using Scripts.SkillSystem.Skills;
using Chipmunk.ComponentContainers;
using Cysharp.Threading.Tasks;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Manage;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemySkillState : EnemyState
    {
        private ActiveSkillComponent _skillCompo;
        private ActiveSkill _currentSkill;
        private IUseStateSkill _stateSkill;
        private static readonly int _skillHash = Animator.StringToHash("SkillIndex");
        public EnemySkillState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _skillCompo = container.Get<ActiveSkillComponent>(true);
        }
        public override async void Enter()
        {
            await UniTask.NextFrame();//정확히 같은 프레임에 스킬 사용하면 안됨(애니메이션)
            base.Enter();
            Debug.Assert(_skillCompo != null && _skillCompo.CurrentSkill != null && _skillCompo.CurrentSkill is IUseStateSkill,
                "CurrentSkill is null but you are in skill state");
            _movement.SetStop(true);
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
                _enemy.ChangeState(EnemyStateEnum.Aim);
        }
        public override void Exit()
        {
            base.Exit();
            _movement.SetStop(false);
            _currentSkill.EndSkill();
            _animatorTrigger.OnCastSkillTrigger -= HandleSkillCast;
        }
    }
}
