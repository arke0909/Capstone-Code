using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.FSM;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.FSM
{
    public class EnemyStateMachineBehavior : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField] private EnemyStateEnum _initialState;
        [SerializeField] private StateMachine<EnemyStateEnum> _stateMachine;
        public ComponentContainer ComponentContainer { get; set; }
        public StateMachine<EnemyStateEnum> StateMachine => _stateMachine;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
        }

        private void Update()
        {
            _stateMachine?.UpdateStateMachine();
        }

        private void OnDestroy()
        {
            ResetStateMachine();
        }

        public void ChangeState(EnemyStateEnum newState, bool forced = false)
        {
            _stateMachine?.ChangeState(newState, forced);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            ResetStateMachine();
            if (eventData.EnemyData == null || eventData.EnemyData.stateDatas == null ||
                eventData.EnemyData.stateDatas.Length == 0)
                return;

            _stateMachine = new StateMachine<EnemyStateEnum>(ComponentContainer, eventData.EnemyData.stateDatas);
            _stateMachine?.ChangeState(_initialState);
        }

        public void ResetStateMachine()
        {
            _stateMachine?.Dispose();
            _stateMachine = null;
        }
    }
}
