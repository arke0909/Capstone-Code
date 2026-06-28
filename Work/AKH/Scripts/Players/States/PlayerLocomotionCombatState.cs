using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Scripts.Combat.Datas;
using Scripts.Entities.Vitals;
using UnityEngine;

namespace Scripts.Players.States
{
    public abstract class PlayerLocomotionCombatState : PlayerCombatState
    {
        private StaminaCompo _staminaCompo;
        private bool _isSound;
        protected PlayerLocomotionCombatState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _staminaCompo = container.Get<StaminaCompo>();
        }
        public override void Update()
        {
            base.Update();

            if (TryEnterAttackState())
                return;

            Vector2 movement = _player.PlayerInput.MovementKey;
            if (_player.PlayerInput.AimKey)
                _player.ChangeState(PlayerStateEnum.Aim);
            else if (_player.PlayerInput.SprintKey && _player.PlayerInput.MovementKey != Vector2.zero && _staminaCompo.CurrentValue > 3f)
                _player.ChangeState(PlayerStateEnum.Sprint);
            else if (movement.magnitude > _inputThreshold)
                _player.ChangeState(PlayerStateEnum.Walk);
            else if (movement.magnitude <= _inputThreshold)
                _player.ChangeState(PlayerStateEnum.Idle);
        }

        private bool TryEnterAttackState()
        {
            if (!_player.PlayerInput.AttackKey
                || _attackable == null)
            {
                _isSound = false;
                return false;
            }

            if (_attackable.CurrentAttackableState == AttackableState.CanAttack)
            {
                if (_attackable.UsesAnimationAttackTrigger)
                    _player.PlayerInput.ConsumeAttack();

                _player.ChangeState(PlayerStateEnum.Attack);
                return true;
            }

            if (_attackable.CurrentAttackableState == AttackableState.NeedAmmo && !_isSound)
            {
                EventBus.Raise(new NoAmmoSoundEvent());
                _isSound = true;
            }

            return false;
        }
    }
}
