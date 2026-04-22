using Chipmunk.ComponentContainers;
using Code;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Scripts.Enemies.EnemyBehaviours;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class SprintToTargetBehaviour : EnemyBehaviour
    {
        private NavMovement _movement;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _movement = enemy.Get<NavMovement>();
        }
        public override void Execute()
        {
            Vector3 targetPos = _enemy.TargetProvider.LastTargetPosition;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _movement.SetDestinationForce(hit.position);
                _enemy.ChangeState(EnemyStateEnum.SprintTo);
            }
            else
            {
                SetCooldown();
            }
        }
    }
}
