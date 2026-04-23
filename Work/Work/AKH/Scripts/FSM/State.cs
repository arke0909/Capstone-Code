using Chipmunk.ComponentContainers;
using Scripts.Entities;

namespace Scripts.FSM
{
    public abstract class State
    {
        protected ComponentContainer _container;
        protected int _animationHash;
        protected EntityAnimator _animator;
        protected EntityAnimatorTrigger _animatorTrigger;
        protected bool _isTriggerCall;

        protected State(ComponentContainer container, int animationHash)
        {
            _container = container;
            _animationHash = animationHash;
            _animator = _container.GetCompo<EntityAnimator>();
            _animatorTrigger = _container.GetCompo<EntityAnimatorTrigger>();
        }

        public virtual void Enter()
        {
            _animator.SetParam(_animationHash, true);
            _isTriggerCall = false;
            _animatorTrigger.OnAnimationEndTrigger += AnimationEndTrigger;
        }

        public virtual void Update(){ }

        public virtual void Exit()
        {
            _animatorTrigger.OnAnimationEndTrigger -= AnimationEndTrigger;
            _animator.SetParam(_animationHash, false);
        }

        public virtual void AnimationEndTrigger() => _isTriggerCall = true;
    }
}