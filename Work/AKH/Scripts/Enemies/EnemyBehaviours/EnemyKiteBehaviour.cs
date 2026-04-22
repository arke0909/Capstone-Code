using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Entities.Enemies.FSM.BehaviourState; // For EnemyMoveState
using Scripts.FSM; // For StateMachine
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemyKiteBehaviour : EnemyBehaviour
    {
        //[SerializeField] private float minKiteDistance = 5f; // 플레이어가 이 거리 안으로 들어오면 카이팅 시작
        [SerializeField] private float targetKiteDistance = 10f; // 카이팅으로 유지하려는 목표 거리
        private CharacterNavMovement _movement;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _movement = enemy.Get<CharacterNavMovement>();
        }

        public override void Execute()
        {
            Vector3 playerPos = _enemy.TargetProvider.LastTargetPosition;
            Vector3 enemyPos = _enemy.transform.position;

            // 플레이어 반대 방향으로 멀어지는 벡터
            Vector3 directionAwayFromPlayer = (enemyPos - playerPos).normalized;

            // 목표 카이팅 거리를 유지하기 위한 지점 계산
            Vector3 targetPos = playerPos + directionAwayFromPlayer * targetKiteDistance;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _movement.SetDestinationForce(hit.position);
                _movement.MoveType = NavMoveType.Aim; // 걷는 로직으로 수정
                _enemy.ChangeState(EnemyStateEnum.MoveTo);
            }
            else
            {
                // 이동 불가능 시 쿨다운 설정
                _enemy.ChangeState(EnemyStateEnum.Aim);
            }

            SetCooldown();
        }
    }
}