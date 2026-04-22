using Chipmunk.GameEvents;

namespace Code.SHS.Entities.Enemies.Events
{
    public struct BossCombatEnteredEvent : IEvent
    {
        public Boss Boss { get; }
        public BossCombatEnteredEvent(Boss boss)
        {
            Boss = boss;
        }
    }
}