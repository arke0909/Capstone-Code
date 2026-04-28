using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Entities.Enemies.Events.Local;
using System.Collections.Generic;
using UnityEngine;

namespace Code.SHS.Entities.Enemies
{
    public class EnemyStatSpawner : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        private StatBehavior _statBehavior;
        private readonly Dictionary<string, float> _defaultBaseValues = new();
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _statBehavior = componentContainer.Get<StatBehavior>(true);
            CacheDefaultStats();
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            ResetRuntimeStats();
            if (eventData.EnemyData == null || eventData.EnemyData.statOverrides == null)
                return;

            foreach (var statOverride in eventData.EnemyData.statOverrides)
            {
                if (_statBehavior.TryGetStat(statOverride.Stat, out var stat))
                    statOverride.ApplyOverride(stat);
            }
        }

        public void ResetRuntimeStats()
        {
            _statBehavior?.CleanAllModifier();
            if (_statBehavior == null)
                return;

            foreach (StatSO stat in _statBehavior.GetAllStats())
            {
                if (stat == null || string.IsNullOrWhiteSpace(stat.statName))
                    continue;

                if (_defaultBaseValues.TryGetValue(stat.statName, out float baseValue))
                {
                    stat.BaseValue = baseValue;
                }
            }
        }

        private void CacheDefaultStats()
        {
            _defaultBaseValues.Clear();
            if (_statBehavior == null)
                return;

            foreach (StatSO stat in _statBehavior.GetAllStats())
            {
                if (stat == null || string.IsNullOrWhiteSpace(stat.statName))
                    continue;

                _defaultBaseValues[stat.statName] = stat.BaseValue;
            }
        }
    }
}
