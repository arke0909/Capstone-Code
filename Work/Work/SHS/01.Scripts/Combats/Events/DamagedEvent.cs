using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Combats.Events
{
    public struct DamagedEvent : ILocalEvent
    {
        public DamageData DamageData { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public Entity Target { get; }
        public Entity Dealer { get; }

        public DamagedEvent(Entity target, DamageData damageData, Vector3 hitPoint, Vector3 hitNormal, Entity dealer)
        {
            Target = target;
            DamageData = damageData;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Dealer = dealer;
        }
    }
}