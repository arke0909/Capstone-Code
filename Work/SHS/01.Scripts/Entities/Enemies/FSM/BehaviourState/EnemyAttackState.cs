using Chipmunk.ComponentContainers;
using Scripts.Combat.Datas;
using Scripts.Enemies.States;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemyAttackContext
    {
        public int attackCount;
    }
    public class EnemyAttackState : EnemyExecuteBehaviourState
    {
        private IAttackable _attackable;
        private EnemyAttackContext _attackContext;
        public override float ExecuteTimer => 0;

        public EnemyAttackState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _attackContext = new() { attackCount = 0 };
            Blackboard.Set("AttackContext", _attackContext);
        }

        public override void Enter()
        {
            base.Enter();
            _attackable = _attackCompo.CurrentAttackable;
            if (_attackable == null || _attackContext.attackCount <= 0)
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
                return;
            }

            if (_attackable.UsesAnimationAttackTrigger)
            {
                _attackContext.attackCount--;
                _animatorTrigger.OnDamageCastTrigger += HandleDamageCast;
            }

            _attackable.EnterAttack();
            _behaviourManager.CurrentBehaviour?.SetCooldown();
            _movement.SetStop(false);
        }

        public override void Update()
        {
            if (_attackable == null)
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
                return;
            }

            if (_attackable.UsesAnimationAttackTrigger)
                UpdateAnimationAttack();
            else
                UpdateImmediateAttack();

            base.Update();
            UpdateMovementAnimation();
        }

        private void UpdateAnimationAttack()
        {
            if (_attackable.CanAttack() && _attackContext.attackCount > 0)
                _enemy.ChangeState(EnemyStateEnum.Attack, true);
            else if (_isTriggerCall && _attackContext.attackCount <= 0)
                _enemy.ChangeState(EnemyStateEnum.Idle);
        }

        private void UpdateImmediateAttack()
        {
            if (_enemy.TargetProvider.CurrentTarget != null)
                _movement.LookAtTarget(_enemy.TargetProvider.CurrentTarget.transform.position);

            bool wantsAttack = _attackContext.attackCount > 0 && _attackable.CanAttack();
            if (wantsAttack)
                _attackContext.attackCount--;

            _attackable.UpdateAttack(new AttackContext(wantsAttack, _isTriggerCall, true));
        }

        private void HandleDamageCast()
        {
            if (_enemy.TargetProvider.CurrentTarget != null)
                _movement.LookAtTarget(_enemy.TargetProvider.CurrentTarget.transform.position);
            _attackable?.AttackTrigger();
        }

        public override void Exit()
        {
            base.Exit();
            _animatorTrigger.OnDamageCastTrigger -= HandleDamageCast;
            _attackable?.EndAttack();
        }
    }
}
