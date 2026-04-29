using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using UnityEngine;

namespace SHS.Scripts.Entities.Levels
{
    public class LevelComponent : MonoBehaviour, IContainerComponent
    {
        [SerializeField, Min(0)] private int _initialLevel;

        private LocalEventBus _localEventBus;
        private int _currentLevel;

        public int CurrentLevel => _currentLevel;
        public ComponentContainer ComponentContainer { get; set; }

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _localEventBus = componentContainer.Get<LocalEventBus>();
            ResetLevel();

            Debug.Assert(_localEventBus != null, $"{gameObject.name} has no LocalEventBus component.");
        }

        public void ResetLevel()
        {
            _currentLevel = Mathf.Max(0, _initialLevel);
        }

        public void SetLevel(int level)
        {
            int nextLevel = Mathf.Max(0, level);
            if (_currentLevel == nextLevel)
                return;

            int previousLevel = _currentLevel;
            _currentLevel = nextLevel;
            _localEventBus?.Raise(new LevelUpEvent(previousLevel, _currentLevel));
        }
    }
}
