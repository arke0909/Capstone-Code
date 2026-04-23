using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Scripts.Enemies.EnemyBehaviourConditions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviours
{
    public enum ConditionType
    {
        AnyAreTrue,
        AllAreTrue
    }
    public abstract class EnemyBehaviour : MonoBehaviour
    {
        [field: SerializeField] public int Priority { get; set; }
        [field: SerializeField] public List<EnemyStateEnum> TargetStates { get; private set; }
        [SerializeField] protected float cooldown;
        [SerializeField] private ConditionType conditionType;
        [SerializeReference]
        private List<EnemyBehaviourCondition> conditions = new();
        protected float _cooldownTimer;
        protected Enemy _enemy;
        public virtual void Init(Enemy enemy)
        {
            _enemy = enemy;
            conditions = conditions.Where(item => item != null).ToList();
            foreach (var condition in conditions)
                condition.Init(enemy);
        }
        protected virtual void Update()
        {
            if (_cooldownTimer >= 0)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0)
                    _cooldownTimer = 0;
            }
        }
        public void SetCooldown()
        {
            _cooldownTimer = cooldown;
        }
        public bool Condition()
        {
            bool success = Mathf.Approximately(_cooldownTimer, 0);
            switch (conditionType)
            {
                case ConditionType.AnyAreTrue:
                    foreach (EnemyBehaviourCondition condition in conditions)
                        success |= condition.Condition();
                    break;
                case ConditionType.AllAreTrue:
                    foreach (EnemyBehaviourCondition condition in conditions)
                        success &= condition.Condition();
                    break;
            }
            return success;
        }
        public abstract void Execute();
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            conditions.ForEach(condition => condition.DrawGizmos(transform));
        }
#endif
    }
}