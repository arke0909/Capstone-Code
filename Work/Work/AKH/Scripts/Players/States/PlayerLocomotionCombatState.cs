using Chipmunk.ComponentContainers;
using Scripts.Entities.Vitals;
using UnityEngine;

namespace Scripts.Players.States
{
    public abstract class PlayerLocomotionCombatState : PlayerCombatState
    {
        private StaminaCompo _staminaCompo;
        protected PlayerLocomotionCombatState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _staminaCompo = container.Get<StaminaCompo>();
        }
        public override void Update()
        {
            base.Update();
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
    }
}
