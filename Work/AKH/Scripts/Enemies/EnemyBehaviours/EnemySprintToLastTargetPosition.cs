using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Targetings.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemySprintToLastTargetPosition : EnemyBehaviour
    {
        private TargetProvider _targetProvider;
        private CharacterNavMovement _movement;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _targetProvider = enemy.Get<TargetProvider>();
            _movement = enemy.Get<CharacterNavMovement>();
        }
        public override void Execute()
        {
            Vector3 targetPos = _targetProvider.LastTargetPosition;
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _movement.SetDestinationForce(targetPos);
                _enemy.ChangeState(Code.SHS.Entities.Enemies.FSM.EnemyStateEnum.SprintTo);
            }
        }
    }
}
