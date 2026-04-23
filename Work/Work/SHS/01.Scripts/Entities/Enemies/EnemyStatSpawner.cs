using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Entities.Enemies.Events.Local;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public class EnemyStatSpawner : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        private StatBehavior _statBehavior;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _statBehavior = componentContainer.Get<StatBehavior>(true);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            foreach (var statOverride in eventData.EnemyData.statOverrides)
            {
                if (_statBehavior.TryGetStat(statOverride.Stat, out var stat))
                    statOverride.ApplyOverride(stat);
            }
        }
    }
}