using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Targetings.Enemies;
using UnityEngine;
using UnityEngine.AI;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class SprintToTargetRadiusBehaviour : EnemyBehaviour
    {
        [SerializeField, Min(0f)] private float pursuitDuration = 3f;
        [SerializeField, Min(0f)] private float targetRadius = 5f;
        [SerializeField, Min(0.01f)] private float navMeshSampleRadius = 2f;
        [SerializeField] private EnemyStateEnum stateOnCancelled = EnemyStateEnum.Aim;

        private CharacterNavMovement _movement;
        private TargetProvider _targetProvider;
        private EnemyBehaviourManager _behaviourManager;
        private float _pursuitTimer;
        private bool _isPursuing;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _movement = enemy.Get<CharacterNavMovement>();
            _targetProvider = enemy.Get<TargetProvider>();
            _behaviourManager = enemy.Get<EnemyBehaviourManager>();
        }

        protected override void Update()
        {
            base.Update();

            if (!_isPursuing || _enemy == null)
                return;

            if (_behaviourManager.CurrentBehaviour != this)
            {
                _isPursuing = false;
                return;
            }

            var stateMachine = _enemy.StateMachineBehavior?.StateMachine;
            if (stateMachine == null)
            {
                _isPursuing = false;
                return;
            }

            EnemyStateEnum currentState = stateMachine.CurrentStateEnum;
            if (currentState != EnemyStateEnum.SprintTo && currentState != EnemyStateEnum.Chase)
            {
                _isPursuing = false;
                return;
            }

            _pursuitTimer -= Time.deltaTime;
            Vector3 targetPosition = GetTargetPosition();
            if (_pursuitTimer <= 0f || IsWithinTargetRadius(targetPosition))
            {
                CancelPursuit();
                return;
            }

            // SprintTo가 이전 목적지에 먼저 도착해 Chase로 돌아온 경우에도
            // 현재 플레이어 위치를 기준으로 질주를 이어 간다.
            if (currentState == EnemyStateEnum.Chase)
            {
                if (!TrySetDestination(targetPosition, true))
                {
                    CancelPursuit();
                    return;
                }

                _enemy.ChangeState(EnemyStateEnum.SprintTo);
                return;
            }

            TrySetDestination(targetPosition, false);
        }

        public override void Execute()
        {
            Vector3 targetPosition = GetTargetPosition();
            if (pursuitDuration <= 0f || IsWithinTargetRadius(targetPosition))
            {
                CancelPursuit();
                return;
            }

            if (!TrySetDestination(targetPosition, true))
            {
                CancelPursuit();
                return;
            }

            _pursuitTimer = pursuitDuration;
            _isPursuing = true;
            _enemy.ChangeState(EnemyStateEnum.SprintTo);
        }

        private Vector3 GetTargetPosition()
        {
            if (_targetProvider.Target != null)
                return _targetProvider.Target.transform.position;

            if (_targetProvider.CurrentTarget != null)
                return _targetProvider.CurrentTarget.transform.position;

            return _targetProvider.LastTargetPosition;
        }

        private bool IsWithinTargetRadius(Vector3 targetPosition)
        {
            Vector3 offset = targetPosition - _enemy.transform.position;
            offset.y = 0f;
            float radius = Mathf.Max(0f, targetRadius);
            return offset.sqrMagnitude <= radius * radius;
        }

        private bool TrySetDestination(Vector3 targetPosition, bool force)
        {
            if (!force)
            {
                _movement.SetDestination(targetPosition);
                return true;
            }

            float sampleRadius = Mathf.Max(0.01f, navMeshSampleRadius);
            if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
                return false;

            return _movement.SetDestinationForce(hit.position);
        }

        private void CancelPursuit()
        {
            _isPursuing = false;
            _pursuitTimer = 0f;
            _movement.SetDestinationForce(_enemy.transform.position);
            _movement.SetStop(true);
            SetCooldown();
            _enemy.ChangeState(stateOnCancelled);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center = transform.position;
            if (_targetProvider != null)
                center = GetTargetPosition();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, Mathf.Max(0f, targetRadius));
        }
#endif
    }
}
