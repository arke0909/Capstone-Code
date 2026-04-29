using Chipmunk.Library.Utility.GameEvents.Local;

namespace SHS.Scripts.Entities.Levels
{
    public struct LevelUpEvent : ILocalEvent
    {
        public int PreviousLevel { get; }
        public int CurrentLevel { get; }

        public LevelUpEvent(int previousLevel, int currentLevel)
        {
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
        }
    }
}
