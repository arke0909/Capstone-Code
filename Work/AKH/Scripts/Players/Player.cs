using Chipmunk.ComponentContainers;
using DewmoLib.Dependencies;
using Scripts.Entities;
using Scripts.FSM;
using Scripts.Players.States;
using System;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Combat;
using UnityEngine;
using SHS.Scripts.Combats.Events;
using SHS.Scripts.NoiseSystems;

namespace Scripts.Players
{
    public interface IStateEntity
    {
        void ChangeState(StateDataSO stateData);
    }

    public class Player : Entity, IAfterInitialze, IDependencyProvider, IStateEntity
    {
        [field: SerializeField] public PlayerInputSO PlayerInput { get; private set; }
        private StateMachine<PlayerStateEnum> _stateMachine;
        [SerializeField] private StateDataSO[] stateDatas;
        [SerializeField] private NoiseGenerator _noiseGenerator;

        [Provide]
        public Player GetPlayer() => this;
        public NoiseGenerator NoiseGenerator => _noiseGenerator;

        public StateMachine<PlayerStateEnum> StateMachine => _stateMachine;

        private LocalEventBus _localEventBus;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            base.OnInitialize(componentContainer);
            _stateMachine = new(componentContainer, stateDatas);
            _localEventBus = componentContainer.Get<LocalEventBus>();
        }

        private void Update()
        {
            _stateMachine?.UpdateStateMachine();
            if (Input.GetKeyDown(KeyCode.P))
                Stun(3f);
        }

        public void ChangeState(PlayerStateEnum newState, bool forced = false)
            => _stateMachine?.ChangeState(newState, forced);

        public void AfterInitialize()
        {
            ChangeState(PlayerStateEnum.Idle);
        }

        public void ChangeState(StateDataSO stateData)
        {
            if (Enum.TryParse<PlayerStateEnum>(stateData.enumName, out var newState))
            {
                ChangeState(newState);
            }
        }

        public override void Stun(float duration)
        {
            var stunState = StateMachine.GetState<PlayerStunState>(PlayerStateEnum.Stun);
            stunState?.SetStunDuration(duration);
            ChangeState(PlayerStateEnum.Stun, true);
            _localEventBus.Raise(new StunnedEvent(duration));
        }
    }
}