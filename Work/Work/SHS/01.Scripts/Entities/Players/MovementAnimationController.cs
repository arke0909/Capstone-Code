using System;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Entities.Players
{
    public class MovementAnimationController : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private float smoothingTime = 0.1f;
        protected static int _xHash = Animator.StringToHash("X");
        protected static int _zHash = Animator.StringToHash("Z");
        private EntityAnimator _animator;
        private Vector2 targetVelocity;
        private Vector2 currentVelocity;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _animator = GetComponent<EntityAnimator>();
        }

        private void Update()
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, (1 / smoothingTime) * Time.deltaTime);
            _animator.SetParam(_xHash, currentVelocity.x);
            _animator.SetParam(_zHash, currentVelocity.y);
        }

        public void SetMoveDirection(Vector3 direction)
        {
            float forwardDot = Vector3.Dot(transform.forward, direction);
            float rightDot = Vector3.Dot(transform.right, direction);
            targetVelocity = new Vector2(rightDot, forwardDot);
        }
    }
}