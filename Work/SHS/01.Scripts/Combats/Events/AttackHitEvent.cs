using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Combat;

namespace SHS.Scripts.Combats.Events
{
    public struct AttackHitEvent : ILocalEvent
    {
        public IDamageable Target { get; }
        public DamageContext DamageContext { get; }

        public AttackHitEvent(IDamageable target, DamageContext damageContext)
        {
            Target = target;
            DamageContext = damageContext;
        }
    }
}
