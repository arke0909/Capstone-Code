using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Entities.Enemies.FSM.BehaviourState; // For EnemyMoveState
using Scripts.FSM; // For StateMachine
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemyFlankBehaviour : EnemyBehaviour
    {
        [SerializeField] private float flankDistance = 8f; // 플레이어로부터 떨어질 거리

        private CharacterNavMovement _movement;
        private bool _flankLeft = true; // 좌우 플랭크 방향

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _movement = enemy.Get<CharacterNavMovement>();
            _flankLeft = Random.value > 0.5f; // 초기 방향 랜덤 설정
        }

        public override void Execute()
        {
            Vector3 playerPos = _enemy.TargetProvider.LastTargetPosition;
            Vector3 enemyPos = _enemy.transform.position;
            
            // 플레이어로부터의 방향 벡터 (y축 무시)
            Vector3 directionToPlayerFlat = (playerPos - enemyPos);
            directionToPlayerFlat.y = 0;
            directionToPlayerFlat.Normalize();

            // 플레이어를 중심으로 좌/우 90도 회전된 방향 계산
            Vector3 flankDirection = Quaternion.Euler(0, _flankLeft ? -90 : 90, 0) * directionToPlayerFlat;
            
            // 최종 목표 지점: 플레이어의 측면 일정 거리
            Vector3 targetPos = playerPos + flankDirection * flankDistance;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _movement.SetDestinationForce(hit.position);
                _enemy.ChangeState(EnemyStateEnum.SprintTo);
                _flankLeft = !_flankLeft; // 다음 플랭크는 반대 방향으로
            }
            else
            {
                // 이동 불가능 시 방향 반전 및 쿨다운 설정
                _flankLeft = !_flankLeft;
                SetCooldown();
                //_enemy.ChangeState(EnemyStateEnum.Aim);
            }
        }
    }
}
