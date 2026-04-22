using Chipmunk.GameEvents;

namespace Work.LKW.Code.Events
{
    public struct PlayerHealEvent : IEvent
    {
        public int HealAmount;

        public PlayerHealEvent(int healAmount)
        {
            HealAmount = healAmount;
        }
    }
}