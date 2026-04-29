using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Entities.Levels.Growths
{
    public class GrowthBehavior : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<LevelUpEvent>
    {
        [SerializeField] private GrowthTableSO _growthTableSO;
        private Entity _entity;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _entity = this.Get<Entity>(true);
        }

        public void SetGrowthTable(GrowthTableSO growthTableSO)
            => _growthTableSO = growthTableSO;

        public void OnLocalEvent(LevelUpEvent eventData)
        {
            _growthTableSO?.ApplyGrowths(_entity, eventData.CurrentLevel);
        }
    }
}
