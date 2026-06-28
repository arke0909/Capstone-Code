using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Animations
{
    public class TurnInPlaceController : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private float turnStartAngle = 55f;
        [SerializeField] private float turnStopAngle = 5f;
        [SerializeField] private string turnLeftBool = "TurnLeft";
        [SerializeField] private string turnRightBool = "TurnRight";

        private EntityAnimator _animator;
        private int _turnDirection;
        private int _turnLeftHash;
        private int _turnRightHash;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _animator = componentContainer.Get<EntityAnimator>();
            _turnLeftHash = Animator.StringToHash(turnLeftBool);
            _turnRightHash = Animator.StringToHash(turnRightBool);
            _animator.OnAnimatorMoveEvent.AddListener(HandleAnimatorMove);
            StopTurn();
        }

        public void UpdateTurn(Vector3 aimDirection)
        {
            aimDirection.y = 0f;
            if (aimDirection.sqrMagnitude < 0.001f)
            {
                StopTurn();
                return;
            }

            float signedAngle = Vector3.SignedAngle(transform.forward, aimDirection.normalized, Vector3.up);
            float absAngle = Mathf.Abs(signedAngle);

            if (_turnDirection != 0)
            {
                if (absAngle <= turnStopAngle || Mathf.Sign(signedAngle) != _turnDirection)
                    StopTurn();

                return;
            }

            if (absAngle < turnStartAngle)
                return;

            _turnDirection = signedAngle > 0f ? 1 : -1;
            _animator.SetParam(_turnLeftHash, _turnDirection < 0);
            _animator.SetParam(_turnRightHash, _turnDirection > 0);
        }

        public void StopTurn()
        {
            _turnDirection = 0;
            _animator.SetParam(_turnLeftHash, false);
            _animator.SetParam(_turnRightHash, false);
        }

        private void HandleAnimatorMove(Vector3 _, Quaternion rotationDelta)
        {
            if (_turnDirection == 0)
                return;

            transform.rotation *= rotationDelta;
        }
    }
}
