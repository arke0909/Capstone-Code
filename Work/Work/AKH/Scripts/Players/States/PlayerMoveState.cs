using Chipmunk.ComponentContainers;
using Code.ETC;
using SHS.Scripts.Crosshairs;
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

            Vector3 moveDir = SetMovementWithCam(_player.PlayerInput.MovementKey);
            _movement.SetMovementDirection(moveDir);
            bool isIdle = _movement.MoveType == MoveType.Idle;
            
            if (_myMoveType != MoveType.Sprint && !isIdle)
            {
                Vector3 direction = _movement.Direction;
                Transform transform = _player.transform;
                Vector3 crosshairPos = _aimProvider.GetAimPosition();
                Vector3 dir = (crosshairPos - transform.position).normalized;
                if (Vector3.Distance(crosshairPos, transform.position) > _cursorLimit)
                    _movement.SetRotationInfo(dir, 15);
                _movementAnimationController.SetMoveDirection(direction);
            }
        }

        private Vector3 SetMovementWithCam(Vector2 dir)
        {
            float cameraYRot = Camera.main.transform.eulerAngles.y;
            return Quaternion.Euler(0, cameraYRot, 0) * new Vector3(dir.x, 0, dir.y);
        }

    }
}
