using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Targetings.Enemies;
using System;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    [Serializable]
    public abstract class EnemyBehaviourCondition
    {
        protected Enemy _enemy;
        protected TargetProvider _targetProvider;
        public virtual void Init(Enemy enemy)
        {
            _enemy = enemy;
            _targetProvider = enemy.Get<TargetProvider>();
        }
        public abstract bool Condition();
#if UNITY_EDITOR
        public virtual void DrawGizmos(Transform trm) { }
#endif
    }
}
