using Scripts.Enemies.EnemyBehaviourConditions;
using UnityEngine;

namespace Assets.Work.AKH.Scripts.Enemies.EnemyBehaviourConditions
{
    public class TargetOutRangeCondition : EnemyBehaviourCondition
    {
        [SerializeField] private float range;
        public override bool Condition()
        => _targetProvider.GetTargetDistance() > range;
#if UNITY_EDITOR
        public override void DrawGizmos(Transform trm)
        {
            base.DrawGizmos(trm);
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(trm.position, range);
        }
#endif
    }
}
