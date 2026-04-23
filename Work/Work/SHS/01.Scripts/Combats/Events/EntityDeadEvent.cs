using Chipmunk.Library.Utility.GameEvents.Local;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Combats.Events
{
    public class EntityDeadEvent : ILocalEvent
    {
        public Entity Entity { get; set; }
        public Vector3 HitNormal { get; }
        public Vector3 HitPoint { get; }

        public EntityDeadEvent(Entity entity, Vector3 hitPoint, Vector3 hitNormal)
        {
            this.Entity = entity;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }
}