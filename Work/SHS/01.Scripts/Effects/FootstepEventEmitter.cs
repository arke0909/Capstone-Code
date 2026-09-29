using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class FootstepEventEmitter : MonoBehaviour, IContainerComponent
    {
        private LocalEventBus _localEventBus;
        private Transform _leftFoot;
        private Transform _rightFoot;

        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _localEventBus = componentContainer.Get<LocalEventBus>();

            Animator animator = GetComponent<Animator>();
            if (animator.isHuman == false)
                return;

            _leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        public void RaiseFootstepEvent()
        {
            Vector3 position = transform.position;
            if (_leftFoot != null && _rightFoot != null)
                position = _leftFoot.position.y <= _rightFoot.position.y
                    ? _leftFoot.position
                    : _rightFoot.position;

            _localEventBus.Raise(new FootstepEvent(position, transform.forward));
        }
    }
}
