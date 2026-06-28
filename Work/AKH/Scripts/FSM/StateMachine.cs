using System;
using System.Collections.Generic;
using System.Reflection;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Entities;
using Scripts.FSM.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.FSM
{
    [Serializable]
    public class StateMachine<TEnum> where TEnum : struct, Enum
    {
        public State CurrentState { get; private set; }

        [SerializeField, ReadOnly] public TEnum CurrentStateEnum;

        private Dictionary<TEnum, State> _states;

        private LocalEventBus _localEventBus;

        public StateMachine(ComponentContainer container, StateDataSO[] stateList)
        {
            _localEventBus = container.GetComponent<LocalEventBus>();

            _states = new Dictionary<TEnum, State>();
            foreach (StateDataSO state in stateList)
            {
                Type type = Type.GetType(state.className);
                Debug.Assert(type != null, $"Finding type is null : {state.className}");
                State entityState = Activator.CreateInstance(type, container, state.animationHash)
                    as State;
                _states.Add(Enum.Parse<TEnum>(state.enumName), entityState);
            }
        }

        /// <summary>
        /// 특정 상태를 가져옵니다.
        /// </summary>
        public TState GetState<TState>(TEnum stateName) where TState : State
        {
            if (_states.TryGetValue(stateName, out State state))
            {
                return state as TState;
            }

            return null;
        }

        public void ChangeState(TEnum newStateEnum, bool forced = false)
        {
            State newState = _states.GetValueOrDefault(newStateEnum);
            Debug.Assert(newState != null, $"State is null {newStateEnum}");

            if (!forced && CurrentState == newState)
                return;

            TEnum previousStateEnum = CurrentStateEnum;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentStateEnum = newStateEnum;
            CurrentState.Enter();
            _localEventBus?.Raise(new StateChangedEvent<TEnum>(previousStateEnum, newStateEnum));
        }

        public void UpdateStateMachine()
        {
            CurrentState?.Update();
        }

        public void Dispose()
        {
            CurrentState?.Exit();
        }
    }
}