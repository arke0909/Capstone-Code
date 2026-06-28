using Chipmunk.ComponentContainers;
using Scripts.SkillSystem.Manage;
using Scripts.SkillSystem.Skills;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyMovingSkillState : EnemyState
    {
        private readonly ActiveSkillComponent _skillComponent;
        private MovingSkill _movingSkill;
        private EnemyAimProvider _enemyAimProvider;
        private float _endTime;

        private static readonly int _skillHash = Animator.StringToHash("SkillIndex");

        public EnemyMovingSkillState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _skillComponent = container.Get<ActiveSkillComponent>(true);
            _enemyAimProvider = container.Get<EnemyAimProvider>();
        }

        public override void Enter()
        {
            base.Enter();

            _movingSkill = (MovingSkill)_skillComponent.CurrentSkill;
            _endTime = Time.time + _movingSkill.Duration;

            _movement.SetStop(false);
            _movement.MoveType = NavMoveType.Sprint;
            _movement.SetLookAtTarget(null);
            _movement.SetDestinationForce(_enemyAimProvider.GetWorldAimPosition());

            _animator.SetParam(_skillHash, (int)_movingSkill.AnimType);
            _animatorTrigger.OnCastSkillTrigger += HandleSkillCast;
            _movingSkill.StartSkill();
        }

        public override void Update()
        {
            base.Update();
            UpdateMovementAnimation();

            if (Time.time >= _endTime)
                _enemy.ChangeState(EnemyStateEnum.Aim);
        }

        public override void Exit()
        {
            base.Exit();
            _movement.SetStop(true);
            _movingSkill.EndSkill();
            _animatorTrigger.OnCastSkillTrigger -= HandleSkillCast;
        }

        private void HandleSkillCast()
        {
            _movingSkill.OnSkillTrigger();
        }
    }
}