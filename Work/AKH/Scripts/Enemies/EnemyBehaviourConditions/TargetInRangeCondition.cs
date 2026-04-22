using System;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    [Serializable]
    public class TargetInRangeCondition : EnemyBehaviourCondition
    {
        [SerializeField] private float range;
        public override bool Condition()
        => _targetProvider.GetTargetDistance() < range;
    }
}
