using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    [Serializable]
    public abstract class EnemyBehaviourPriorityCondition : EnemyBehaviourCondition
    {
        [SerializeField] private int priorityOffset;

        public int GetPriorityOffset()
            => Condition() ? priorityOffset : 0;
    }

    [Serializable]
    public class HealthRatioPriorityCondition : EnemyBehaviourPriorityCondition
    {
        [SerializeField] private float minRatio;
        [SerializeField] private float maxRatio = 1f;

        private HealthCompo _healthCompo;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _healthCompo = enemy.Get<HealthCompo>();
        }

        public override bool Condition()
        {
            if (_healthCompo.MaxValue <= 0f)
                return false;

            float ratio = _healthCompo.CurrentValue / _healthCompo.MaxValue;
            return ratio >= minRatio && ratio <= maxRatio;
        }
    }

    [Serializable]
    public class TargetDistancePriorityCondition : EnemyBehaviourPriorityCondition
    {
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;

        public override bool Condition()
        {
            if (_targetProvider.Target == null)
                return false;

            float distance = Vector3.Distance(_enemy.transform.position, _targetProvider.Target.transform.position);
            return distance >= minDistance && distance <= maxDistance;
        }
    }

    [Serializable]
    public class CurrentStatePriorityCondition : EnemyBehaviourPriorityCondition
    {
        [SerializeField] private List<EnemyStateEnum> targetStates = new();

        public override bool Condition()
            => targetStates.Contains(_enemy.StateMachineBehavior.StateMachine.CurrentStateEnum);
    }
}
