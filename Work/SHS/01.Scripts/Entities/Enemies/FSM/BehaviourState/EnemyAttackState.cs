using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Code.Players;
using Scripts.Combat.Datas;
using Scripts.Enemies.States;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Code.SHS.Entities.Enemies.FSM.BehaviourState
{
    public class EnemyAttackState : EnemyExecuteBehaviourState
    {
        private IAttackable _weaponItem;
        private EnemyEquipment _equipment;

        public override float ExecuteTimer => 0;

        public EnemyAttackState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _equipment = container.Get<EnemyEquipment>();
        }

        public override void Enter()
        {
            base.Enter();
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
            if (_isTriggerCall)
                _enemy.ChangeState(EnemyStateEnum.Aim);
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