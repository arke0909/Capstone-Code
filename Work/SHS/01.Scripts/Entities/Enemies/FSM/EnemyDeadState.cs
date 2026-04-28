using Chipmunk.ComponentContainers;
using Code.SHS.Targetings.Enemies;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyDeadState : EnemyState
    {
        const float DestroyDelay = 300f;
        private float _destroyTimer = 0;
        private TargetDetector _targetDetector;

        public EnemyDeadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _targetDetector = container.Get<TargetDetector>();
        }

        public override void Enter()
        {
            base.Enter();
            _destroyTimer = 0f;
            _movement.SetStop(true);
            _movement.SetLookAtTarget(null);
            _movement.enabled = false;
            _targetDetector.enabled = false;
        }

        public override void Update()
        {
            base.Update();
            _destroyTimer += Time.deltaTime;
            if (_destroyTimer >= DestroyDelay)
                _enemy.ReleaseToPool();
        }
    }
}
