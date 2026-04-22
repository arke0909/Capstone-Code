using Chipmunk.Library.Utility.GameEvents.Local;

namespace SHS.Scripts.Combats.Events
{
    public class StunnedEvent : ILocalEvent
    {
        public float StunDuration { get; private set; }

        public StunnedEvent(float stunDuration)
        {
            StunDuration = stunDuration;
        }
    }
}