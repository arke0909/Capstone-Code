using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using SHS.Scripts.Summon.Turrets;
using System;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    [Serializable]
    public class TargetDistanceCondition : EnemyBehaviourCondition
    {
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance = 999f;

        public override bool Condition()
        {
            float distance = _targetProvider.GetTargetDistance();
            return distance >= minDistance && distance <= maxDistance;
        }
    }

    [Serializable]
    public class ActiveTurretCountBelowCondition : EnemyBehaviourCondition
    {
        [SerializeField] private int maxActiveCount = 2;

        private EngineerTurretTracker _turretTracker;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _turretTracker = enemy.Get<EngineerTurretTracker>();
            Debug.Assert(_turretTracker != null, $"{nameof(ActiveTurretCountBelowCondition)} requires {nameof(EngineerTurretTracker)}.");
        }

        public override bool Condition()
            => _turretTracker.ActiveTurretCount < maxActiveCount;
    }

    [Serializable]
    public class HasActiveTurretPriorityCondition : EnemyBehaviourPriorityCondition
    {
        private EngineerTurretTracker _turretTracker;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _turretTracker = enemy.Get<EngineerTurretTracker>();
            Debug.Assert(_turretTracker != null, $"{nameof(HasActiveTurretPriorityCondition)} requires {nameof(EngineerTurretTracker)}.");
        }

        public override bool Condition()
            => _turretTracker.ActiveTurretCount > 0;
    }
}
