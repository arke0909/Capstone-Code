using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Entities;

namespace Code.SHS.Entities.Enemies.Targetings.Events
{
    public struct TargetDetectedEvent : ILocalEvent
    {
        public Entity Target { get;}
        public TargetDetectedEvent(Entity target)
        {
            Target = target;
        }

    }
}