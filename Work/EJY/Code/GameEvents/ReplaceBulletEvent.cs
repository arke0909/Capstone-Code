using Chipmunk.GameEvents;
using Scripts.Combat.Datas;

namespace Code.GameEvents
{
    public struct ReplaceBulletEvent : IEvent
    {
        public BulletItem Bullet { get; }

        public ReplaceBulletEvent(BulletItem bullet)
        {
            Bullet = bullet;
        }
    }

    public struct OffReplaceBulletUI : IEvent
    { }
}