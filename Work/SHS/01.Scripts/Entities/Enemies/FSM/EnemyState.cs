using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Targetings.Events;
using Code.SHS.Targetings.Enemies;
using Scripts.Combat;
using Scripts.Enemies.EnemyBehaviours;
using Scripts.Entities;
using Scripts.FSM;
using System;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public enum EnemyStateEnum
    {
        Idle,
        Patrol,
        Chase,
        Aim,
        Attack,
        Reload,
        Skill,
        AimSkill,
        Reposition,
        MoveTo,
        SprintTo,
        Stun,
        Dead
    }

    public abstract class EnemyState : State
    {
        protected Enemy _enemy;
        protected CharacterNavMovement _movement;
        protected AttackCompo _attackCompo;
        protected EnemyBehaviourManager _behaviourManager;
        protected EnemyInventory _enemyInventory;
        protected TargetProvider _targetProvider;
        protected LocalEventBus _localEventBus;
        protected Entity RemainTarget => _targetProvider.CurrentTarget;
        protected Entity Target => _targetProvider.Target;

        protected static int _xHash = Animator.StringToHash("X");
        protected static int _zHash = Animator.StringToHash("Z");

        protected float _attackRange => _attackCompo.AttackRange;
        public EnemyState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _enemy = container.GetCompo<Enemy>(true);
            _movement = container.GetCompo<CharacterNavMovement>();
            _behaviourManager = container.Get<EnemyBehaviourManager>();
            _enemyInventory = container.Get<EnemyInventory>();
            _attackCompo = container.Get<AttackCompo>();
            _targetProvider = container.Get<TargetProvider>();
            _localEventBus = container.Get<LocalEventBus>();
        }
        public override void Enter()
        {
            base.Enter();
            _localEventBus.Subscribe<TargetLostEvent>(HandleTargetLost);
        }
        public override void Exit()
        {
            base.Exit();
            _localEventBus.Unsubscribe<TargetLostEvent>(HandleTargetLost);
        }
        private void HandleTargetLost(TargetLostEvent @event)
        {
            _enemy.ChangeState(EnemyStateEnum.Idle);
        }

        protected void UpdateMovementAnimation()
        {
            Vector3 velocity = _movement.Velocity;
            if (velocity.sqrMagnitude < 0.01f)
            {
                _animator.SetParam(_xHash, 0f);
                _animator.SetParam(_zHash, 0f);
                return;
            }

            Vector3 direction = velocity.normalized;
            Transform transform = _enemy.transform;

            float forwardDot = Vector3.Dot(transform.forward, direction);
            float rightDot = Vector3.Dot(transform.right, direction);

            _animator.SetParam(_xHash, rightDot);
            _animator.SetParam(_zHash, forwardDot);
        }
    }
}
