using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies.FSM;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Skills
{
    public class RogueStealthSkill : ActiveSkill
    {
        [SerializeField, Min(0f)] private float stealthDuration = 4f;
        [SerializeField, Min(0f)] private float stealthMoveSpeedMultiplier = 1.2f;

        public bool IsStealthed => _isStealthed;

        private CharacterNavMovement _movement;
        private EnemyStateMachineBehavior _stateMachine;
        private readonly Dictionary<Renderer, bool> _rendererState = new();

        private bool _isStealthed;
        private float _remainTime;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _movement = container.Get<CharacterNavMovement>(true);
            _stateMachine = container.Get<EnemyStateMachineBehavior>(true);

            Renderer[] renderers = _owner.GetComponentsInChildren<Renderer>(true);
            _rendererState.Clear();
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer != null && !_rendererState.ContainsKey(renderer))
                    _rendererState.Add(renderer, renderer.enabled);
            }

            _owner.OnHitEvent?.AddListener(BreakStealth);
            _owner.OnDeadEvent?.AddListener(BreakStealth);
        }

        private void OnDestroy()
        {
            if (_owner != null)
            {
                _owner.OnHitEvent?.RemoveListener(BreakStealth);
                _owner.OnDeadEvent?.RemoveListener(BreakStealth);
            }
        }

        public override void StartAndUseSkill()
        {
            if (_isStealthed)
            {
                BreakStealth();
                return;
            }

            _isStealthed = true;
            _remainTime = stealthDuration;
            SetRenderVisible(false);
            ApplyMoveMultiplier();
        }

        public override void EndSkill()
        {
            // Enemy skill state에서 호출돼도 은신은 유지되어야 하므로 아무 것도 하지 않음.
        }

        private void Update()
        {
            if (!_isStealthed)
                return;

            _remainTime -= Time.deltaTime;
            if (_remainTime <= 0f)
            {
                BreakStealth();
                return;
            }

            if (_stateMachine != null &&
                _stateMachine.StateMachine != null &&
                _stateMachine.StateMachine.CurrentStateEnum == EnemyStateEnum.Attack)
            {
                BreakStealth();
            }
        }

        private void BreakStealth()
        {
            if (!_isStealthed)
                return;

            _isStealthed = false;
            _remainTime = 0f;
            SetRenderVisible(true);
            ResetMoveMultiplier();
        }

        private void SetRenderVisible(bool visible)
        {
            foreach (var pair in _rendererState)
            {
                if (pair.Key == null)
                    continue;

                pair.Key.enabled = visible ? pair.Value : false;
            }
        }

        private void ApplyMoveMultiplier()
        {
            if (_movement != null)
                _movement.SpeedMultiplier = stealthMoveSpeedMultiplier;
        }

        private void ResetMoveMultiplier()
        {
            if (_movement != null)
                _movement.SpeedMultiplier = 1f;
        }
    }
}

