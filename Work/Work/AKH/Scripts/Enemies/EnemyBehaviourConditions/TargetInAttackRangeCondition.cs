using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Scripts.Combat;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    public class TargetInAttackRangeCondition : EnemyBehaviourCondition
    {
        private AttackCompo _attackCompo;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _attackCompo = enemy.Get<AttackCompo>();
        }
        public override bool Condition()
        {
            return _attackCompo.AttackRange >= _targetProvider.GetTargetDistance();
        }
    }
}
