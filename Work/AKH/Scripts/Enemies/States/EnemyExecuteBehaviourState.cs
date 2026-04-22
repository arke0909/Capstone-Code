using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.FSM;
using UnityEngine;

namespace Scripts.Enemies.States
{
    public abstract class EnemyExecuteBehaviourState : EnemyState
    {
        public abstract float ExecuteTimer { get; }
        protected float _currentTimer;
        public EnemyExecuteBehaviourState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
        public override void Enter()
        {
            base.Enter();
            _currentTimer = 0;
        }
        public override void Update()
        {
            base.Update();
            if (_currentTimer >= ExecuteTimer)
            {
                _behaviourManager.ExecuteOptimalCurrentState();
                _currentTimer = 0;
            }
            _currentTimer = Mathf.Min(_currentTimer + Time.deltaTime, ExecuteTimer);
        }
    }
}
