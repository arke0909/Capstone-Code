using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Scripts.Combat.Datas;
using Scripts.Enemies.States;
using UnityEngine;
using Code.Items;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemyAttackContext
    {
        public int attackCount;
    }
    public class EnemyAttackState : EnemyExecuteBehaviourState
    {
        private IAttackable _weaponItem;
        private EnemyEquipment _equipment;
        private int _remainAttackCount;
        public override float ExecuteTimer => 0;

        public EnemyAttackState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _equipment = container.Get<EnemyEquipment>();
        }

        public override void Enter()
        {
            base.Enter();
            if (_remainAttackCount <= 0)
            {
                EnemyAttackContext context = Blackboard.GetOrDefault<EnemyAttackContext>("AttackContext");
                _remainAttackCount = context == null ? 1 : context.attackCount;
                context.attackCount = 1;
            }
            _remainAttackCount = Mathf.Max(_remainAttackCount -1,0);
            _animatorTrigger.OnDamageCastTrigger += HandleDamageCast;

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) &&
                item is IAttackable attackable)
                _weaponItem = attackable;
            _weaponItem?.EnterAttack();
            _behaviourManager.CurrentBehaviour?.SetCooldown();
            _movement.SetStop(false);
        }

        public override void Update()
        {
            if (_weaponItem.CanAttack() && _remainAttackCount > 0)//공격 카운트가 남았고 공격 가능한 상태일때
                _enemy.ChangeState(EnemyStateEnum.Attack, true);
            else if(_isTriggerCall && (_remainAttackCount <= 0 || !_weaponItem.CanAttack()))
            {
                _enemy.ChangeState(EnemyStateEnum.Idle);
            }
            base.Update();
            UpdateMovementAnimation();
        }

        private void HandleDamageCast()
        {
            if (_enemy.TargetProvider.CurrentTarget != null)
                _movement.LookAtTarget(_enemy.TargetProvider.CurrentTarget.transform.position);
            _weaponItem?.AttackTrigger();
        }

        public override void Exit()
        {
            base.Exit();
            _animatorTrigger.OnDamageCastTrigger -= HandleDamageCast;
        }
    }
}