using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Code.Players;
using Scripts.Combat.Datas;
using UnityEngine;
using Code.Items;
using SHS.Scripts.Combats;

namespace Scripts.Players.States
{
    public class PlayerAttackState : PlayerMoveState
    {
        private IAttackable _attackable;
        private PlayerEquipment _equipment;
        private DefaultAttack _defaultAttack;

        public PlayerAttackState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Aim;
            _equipment = container.Get<PlayerEquipment>();
            _defaultAttack = container.Get<DefaultAttack>();
            Debug.Assert(_defaultAttack != null, "Player requires DefaultAttack for unarmed attacks.");
        }

        public override void Enter()
        {
            base.Enter();
            _attackable = null;

            Vector3 aimDirection = _aimProvider.GetAimPosition() - _player.transform.position;
            aimDirection.y = 0;
            if (aimDirection.sqrMagnitude > 0f)
            {
                _movement.SetRotationInfo(aimDirection, 0);
                _movement.SetRotation(aimDirection);
            }
            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item))
            {
                if (item is IAttackable attackable)
                    _attackable = attackable;
            }
            else
            {
                _attackable = _defaultAttack;
            }

            if (_attackable?.UsesAnimationAttackTrigger == true)
                _animatorTrigger.OnDamageCastTrigger += HandleDamageCast;

            _attackable?.EnterAttack();
        }

        public override void Update()
        {
            base.Update();
            if (_attackable == null)
            {
                Debug.Log("No weapon to attack with, returning to locomotion state.");
                ReturnToLocomotionState();
                return;
            }

            AttackContext context = new(
                _player.PlayerInput.AttackKey,
                _isTriggerCall,
                _player.PlayerInput.AimKey);

            _attackable.UpdateAttack(context);
            UpdateAttackState(context);
        }

        private void HandleDamageCast()
        {
            _attackable?.AttackTrigger();
        }

        private void UpdateAttackState(AttackContext context)
        {
            if (!_attackable.UsesAnimationAttackTrigger)
            {
                if (!context.WantsAttack || !CanKeepAttackState())
                    ReturnToLocomotionState();
                return;
            }

            if (!context.AnimationEnded)
                return;

            ReturnToLocomotionState();
        }

        private bool CanKeepAttackState()
        {
            AttackableState state = _attackable.CurrentAttackableState;
            return state == AttackableState.CanAttack || state == AttackableState.Delayed;
        }

        private void ReturnToLocomotionState()
        {
            if (_player.PlayerInput.AimKey)
                _player.ChangeState(PlayerStateEnum.Aim);
            else if (_player.PlayerInput.MovementKey.magnitude > _inputThreshold)
                _player.ChangeState(PlayerStateEnum.Walk);
            else
                _player.ChangeState(PlayerStateEnum.Idle);
        }

        public override void Exit()
        {
            base.Exit();
            _animatorTrigger.OnDamageCastTrigger -= HandleDamageCast;
            _attackable?.EndAttack();
        }
    }
}
