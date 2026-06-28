using Chipmunk.ComponentContainers;
using Code.ETC;
using SHS.Scripts.Entities.Players;
using UnityEngine;

namespace Scripts.Players.States
{
    public abstract class PlayerMoveState : PlayerState
    {
        protected MoveType _myMoveType;

        private MovementAnimationController _movementAnimationController;
        protected IAimProvider _aimProvider;
        protected static float _cursorLimit = 1f;
        private const float CombatAimRotationSpeed = 15f;

        public PlayerMoveState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Walk;
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            _movementAnimationController = container.Get<MovementAnimationController>();
        }

        public override void Enter()
        {
            base.Enter();
            _movement.MoveType = _myMoveType;
        }

        public override void Update()
        {
            base.Update();

            if (ShouldProcessManualMovement())
                UpdateManualMovement();
        }

        protected virtual bool ShouldProcessManualMovement() => true;

        protected void UpdateManualMovement()
        {
            Vector2 movementInput = _player.PlayerInput.MovementKey;
            Vector3 moveDir = SetMovementWithCam(movementInput);
            _movement.SetMovementDirection(moveDir);

            bool isIdle = _movement.MoveType == MoveType.Idle;

            if (_myMoveType != MoveType.Sprint)
            {
                Vector3 direction = _movement.Direction;
                Vector3 crosshairPos = _aimProvider.GetAimPosition();
                Vector3 aimDirection = crosshairPos - _player.transform.position;
                aimDirection.y = 0f;

                UpdateAimRotation(aimDirection, movementInput);

                if (!isIdle)
                    _movementAnimationController.SetMoveDirection(direction);
            }
        }

        protected void StopManualMovement()
        {
            _movement.SetMovementDirection(Vector3.zero);
            _movement.StopImmediately();
        }

        protected virtual void UpdateAimRotation(Vector3 aimDirection, Vector2 movementInput)
        {
            if (aimDirection.sqrMagnitude <= _cursorLimit * _cursorLimit)
                return;

            _movement.SetRotationInfo(aimDirection.normalized, CombatAimRotationSpeed);
        }

        private Vector3 SetMovementWithCam(Vector2 dir)
        {
            float cameraYRot = Camera.main.transform.eulerAngles.y;
            return Quaternion.Euler(0, cameraYRot, 0) * new Vector3(dir.x, 0, dir.y);
        }
    }
}