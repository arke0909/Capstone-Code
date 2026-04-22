using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.FSM;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyStateMachineBehavior : MonoBehaviour, IContainerComponent,ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField] private EnemyStateEnum _initialState;
        [SerializeField] private StateMachine<EnemyStateEnum> _stateMachine;
        public ComponentContainer ComponentContainer { get; set; }
        public StateMachine<EnemyStateEnum> StateMachine => _stateMachine;

        public void OnInitialize(ComponentContainer componentContainer)
        {
        }

        private void Update()
        {
            _stateMachine?.UpdateStateMachine();
        }

        private void OnDestroy()
        {
            _stateMachine?.Dispose();
        }

        public void ChangeState(EnemyStateEnum newState, bool forced = false)
        {
            _stateMachine?.ChangeState(newState, forced);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            _stateMachine = new StateMachine<EnemyStateEnum>(ComponentContainer, eventData.EnemyData.stateDatas);
            _stateMachine?.ChangeState(_initialState);
        }
    }
}