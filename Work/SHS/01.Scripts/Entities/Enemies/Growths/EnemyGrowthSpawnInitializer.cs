using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.SHS.Entities.Enemies.Events.Local;
using Code.SHS.Entities.Enemies.Spawns;
using Code.TimeSystem;
using SHS.Scripts.Entities.Levels;
using SHS.Scripts.Entities.Levels.Growths;
using Work.Code.GameEvents;

namespace Code.SHS.Entities.Enemies.Growths
{
    public class EnemyGrowthSpawnInitializer : EnemySpawnInitializer
    {
        private LevelComponent _levelComponent;
        private GrowthBehavior _growthBehavior;

        public override void OnInitialize(ComponentContainer componentContainer)
        {
            _growthBehavior = componentContainer.Get<GrowthBehavior>();
            _levelComponent = componentContainer.Get<LevelComponent>();
            EventBus.Subscribe<DayChangeEvent>(OnDayChanged);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayChangeEvent>(OnDayChanged);
        }

        public override void OnLocalEvent(EnemySpawnEvent eventData)
        {
            base.OnLocalEvent(eventData);
            _levelComponent.ResetLevel();
            _growthBehavior.SetGrowthTable(eventData.EnemyData != null ? eventData.EnemyData.growthTable : null);
            UpdateLevelByDay();
        }


        private void OnDayChanged(DayChangeEvent evt)
        {
            UpdateLevelByDay();
        }

        private void UpdateLevelByDay()
        {
            _levelComponent.SetLevel(TimeController.Instance.CurrentDay);
        }
    }
}
