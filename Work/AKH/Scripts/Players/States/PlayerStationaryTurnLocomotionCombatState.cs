using Chipmunk.ComponentContainers;
using SHS.Scripts.Animations;
using UnityEngine;

namespace Scripts.Players.States
{
    public abstract class PlayerStationaryTurnLocomotionCombatState : PlayerLocomotionCombatState
    {
        private TurnInPlaceController _turnInPlaceController;

        protected PlayerStationaryTurnLocomotionCombatState(ComponentContainer container, int animationHash) : base(
            container, animationHash)
        {
            _turnInPlaceController = container.Get<TurnInPlaceController>(true);
        }

        public override void Exit()
        {
            _turnInPlaceController.StopTurn();
            base.Exit();
        }

        protected override void UpdateAimRotation(Vector3 aimDirection, Vector2 movementInput)
        {
            if (movementInput.magnitude > _inputThreshold)
            {
                _turnInPlaceController.StopTurn();
                base.UpdateAimRotation(aimDirection, movementInput);
                return;
            }

            _movement.StopRotation();

            if (aimDirection.sqrMagnitude <= _cursorLimit * _cursorLimit)
            {
                _turnInPlaceController.StopTurn();
                return;
            }

            _turnInPlaceController.UpdateTurn(aimDirection);
        }
    }
}
