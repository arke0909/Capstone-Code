using Chipmunk.ComponentContainers;
using Code.SHS.Targetings.Enemies;
using Code.StatusEffectSystem;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyDeadState : EnemyState
    {
        const float DestroyDelay = 300f;
        private float _destroyTimer = 0;
        private TargetDetector _targetDetector;
        private EntityStatusEffect _statusCompo;
        public EnemyDeadState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _targetDetector = container.Get<TargetDetector>();
            _statusCompo = container.Get<EntityStatusEffect>();
        }

        public override void Enter()
        {
            base.Enter();
            _destroyTimer = 0f;
            _movement.SetStop(true);
            _movement.SetLookAtTarget(null);
            _movement.enabled = false;
            _targetDetector.enabled = false;
            _statusCompo.ClearStatusEffect();
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
