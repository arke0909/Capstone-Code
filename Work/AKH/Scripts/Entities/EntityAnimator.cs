using Chipmunk.ComponentContainers;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Entities
{
    public class EntityAnimator : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private Animator animator;
        public UnityEvent<Vector3, Quaternion> OnAnimatorMoveEvent;
        public UnityEvent OnControllerChanged;
        public bool ApplyRootMotion
        {
            get => animator.applyRootMotion;
            set => animator.applyRootMotion = value;
        }
        public ComponentContainer ComponentContainer { get; set; }
        private RuntimeAnimatorController _defaultController;
        public void SetParam(int hash, float value, float dampTime) => animator.SetFloat(hash, value, dampTime, Time.deltaTime);
        public void SetParam(int hash, float value) => animator.SetFloat(hash, value);
        public void SetParam(int hash, int value) => animator.SetInteger(hash, value);
        public void SetParam(int hash, bool value) => animator.SetBool(hash, value);
        public void SetParam(int hash) => animator.SetTrigger(hash);
        public void ChangeAnimatorController(RuntimeAnimatorController controller)
        {
            animator.runtimeAnimatorController = controller == null? _defaultController:controller;
            OnControllerChanged?.Invoke();
        }
        public void SetDefaultController() => ChangeAnimatorController(_defaultController);
        public void OffAnimator()
        {
            animator.enabled = false;
        }
        private void OnAnimatorMove()
        {
            OnAnimatorMoveEvent?.Invoke(animator.deltaPosition, animator.deltaRotation);
        }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _defaultController = animator.runtimeAnimatorController;
            ApplyRootMotion = false;
        }
    }
}