using Chipmunk.GameEvents;

namespace Code.SHS.Entities.Enemies.Events
{
    public readonly struct EnemyCombatStateChangedEvent : IEvent
    {
        public Enemy Enemy { get; }
        public bool IsInCombat { get; }

        public EnemyCombatStateChangedEvent(Enemy enemy, bool isInCombat)
        {
            Enemy = enemy;
            IsInCombat = isInCombat;
        }
    }
}
