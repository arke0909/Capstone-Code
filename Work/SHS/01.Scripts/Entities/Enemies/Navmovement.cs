using System;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace Code.SHS.Entities.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMovement : MonoBehaviour, IContainerComponent, IAfterInitialze, IKnockbackable,
        ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected float stopOffset = 0.05f;
        [SerializeField] protected float rotateSpeed = 10f;
        [SerializeField] protected bool isUpdateRotation;
        [SerializeField] protected StatSO moveSpeedStat;

        [Header("Path Update Settings")] [SerializeField]
        protected float pathUpdateInterval = 0.2f;

        [SerializeField] protected float pathUpdateThreshold = 0.25f;

        protected Transform _lookAtTrm;
        protected Vector3 _lastDestination = Vector3.positiveInfinity;
        protected float _lastPathUpdateTime;

        // 비동기 경로 계산용
        private NavMeshPath _pendingPath;
        private Vector3 _pendingDestination;
        private bool _isCalculatingPath;

        public bool IsArrived
        {
            get
            {
                // 경로 계산 중이면 아직 도착하지 않은 것으로 처리
                if (agent.pathPending || _isCalculatingPath)
                    return false;

                // 경로가 없으면 도착한 것으로 처리
                if (!agent.hasPath)
                    return true;

                return agent.remainingDistance <= agent.stoppingDistance + stopOffset;
            }
        }

        public float RemainDistance => agent.pathPending ? -1 : agent.remainingDistance;
        public Vector3 Velocity => agent.velocity;

        public bool UpdateRotation
        {
            get => agent.updateRotation;
            set => agent.updateRotation = value;
        }

        protected float _speedMultiplier = 1f;

        public virtual float SpeedMultiplier
        {
            get => _speedMultiplier;
            set
            {
                _speedMultiplier = value;
                UpdateAgentSpeed();
            }
        }

        public ComponentContainer ComponentContainer { get; set; }

        public virtual void OnInitialize(ComponentContainer componentContainer)
        {
        }

        public virtual void AfterInitialize()
        {
            moveSpeedStat = this.GetContainerComponent<StatOverrideBehavior>().GetStat(moveSpeedStat);
            UpdateAgentSpeed();
        }

        protected virtual void UpdateAgentSpeed()
        {
            agent.speed = moveSpeedStat.Value * _speedMultiplier;
        }

        protected virtual void HandleMoveSpeedChange(StatSO stat, float currentvalue, float previousvalue)
        {
            UpdateAgentSpeed();
        }

        protected virtual void Update()
        {
            // 비동기 경로 계산 완료 체크
            CheckPendingPath();

            if (_lookAtTrm != null)
            {
                LookAtTarget(_lookAtTrm.position);
            }
            else if (agent.hasPath && agent.isStopped == false)
            {
                LookAtTarget(agent.steeringTarget);
            }
        }

        public Quaternion LookAtTarget(Vector3 target, bool isSmooth = true)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0;
            Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);

            if (isSmooth)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    lookRotation, Time.deltaTime * rotateSpeed);
            }
            else
            {
                transform.rotation = lookRotation;
            }

            return lookRotation;
        }

        public void SetLookAtTarget(Transform target)
        {
            _lookAtTrm = target;
            UpdateRotation = _lookAtTrm == null;
        }

        public void Move(Vector3 position) => agent.Move(position);
        public void SetStop(bool isStop)
        {
            if (!agent.isActiveAndEnabled)
                return;

            if (!agent.isOnNavMesh)
                return;
            agent.isStopped = isStop;
        }
        public void SetVelocity(Vector3 velocity) => agent.velocity = velocity;
        public virtual void SetSpeed(float speed) => agent.speed = speed;

        public void SetDestination(Vector3 destination)
        {
            float timeSinceLastUpdate = Time.time - _lastPathUpdateTime;
            float distanceFromLastDest = Vector3.Distance(_lastDestination, destination);

            bool shouldUpdate = timeSinceLastUpdate >= pathUpdateInterval
                                || distanceFromLastDest > pathUpdateThreshold;

            if (shouldUpdate && !_isCalculatingPath)
            {
                // 비동기로 경로 계산 시작 (기존 경로 유지하면서)
                CalculatePathAsync(destination);
            }
        }

        private void CalculatePathAsync(Vector3 destination)
        {
            _pendingPath ??= new NavMeshPath();
            _pendingDestination = destination;
            _isCalculatingPath = true;

            // 비동기 경로 계산 시작 - 기존 움직임은 계속 유지됨
            agent.CalculatePath(destination, _pendingPath);
        }

        private void CheckPendingPath()
        {
            if (!_isCalculatingPath) return;

            // 경로 계산이 완료되었는지 확인
            if (_pendingPath.status == NavMeshPathStatus.PathComplete)
            {
                // 유효한 경로면 적용
                agent.SetPath(_pendingPath);
                _lastDestination = _pendingDestination;
                _lastPathUpdateTime = Time.time;
                _isCalculatingPath = false;
            }
            else if (_pendingPath.status == NavMeshPathStatus.PathInvalid)
            {
                // 경로 실패 시 그냥 직접 이동 시도
                agent.SetDestination(_pendingDestination);
                
                _lastDestination = _pendingDestination;
                _lastPathUpdateTime = Time.time;
                _isCalculatingPath = false;
            }
            // PathPartial인 경우도 적용 (부분 경로라도 사용)
            else if (_pendingPath.status == NavMeshPathStatus.PathPartial)
            {
                agent.SetPath(_pendingPath);
                _lastDestination = _pendingDestination;
                _lastPathUpdateTime = Time.time;
                _isCalculatingPath = false;
            }
        }

        /// <summary>
        /// 강제로 경로를 재계산합니다 (쿨다운 무시, 동기)
        /// </summary>
        public bool SetDestinationForce(Vector3 destination)
        {
            if (agent.SetDestination(destination))
            {
                _isCalculatingPath = false;
                _lastDestination = destination;
                _lastPathUpdateTime = Time.time;
                return true;
            }

            agent.SetDestination(transform.position);
            return false;
        }

        /// <summary>
        /// 경로 업데이트 상태를 초기화합니다
        /// </summary>
        public void ResetPathUpdate()
        {
            _lastDestination = Vector3.positiveInfinity;
            _lastPathUpdateTime = 0f;
            _isCalculatingPath = false;
        }

        public virtual void ResetMovementState(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            SetLookAtTarget(null);
            ResetPathUpdate();

            if (!agent.isActiveAndEnabled)
                return;

            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.Warp(position);
            }

            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            enabled = true;
            ResetMovementState(eventData.Position, eventData.Rotation);
        }

        public void WarpToPosition(Vector3 position) => agent.Warp(position);

        public async void KnockBack(Vector3 direction, MovementDataSO kbMovement)
        {
            //여기서 넉백 저항력이 있다면 반영해서 저항해줘야 한다.
            SetStop(true); //네비게이션은 정지시켜주고

            float duration = kbMovement.duration;
            float currentTime = 0;
            float maxSpeed = kbMovement.maxSpeed;
            AnimationCurve moveCurve = kbMovement.moveCurve;

            while (currentTime < duration)
            {
                float normalizeTime = currentTime / duration;
                float currentSpeed = maxSpeed * moveCurve.Evaluate(normalizeTime);
                Vector3 currentMovement = direction * currentSpeed;
                agent.transform.Translate(currentMovement * Time.fixedDeltaTime, Space.World);
                currentTime += Time.fixedDeltaTime;
                await Awaitable.FixedUpdateAsync();
            }

            //여기서 추가 작업을 안해주면 넉백이 이상해진다. 일단 이상하게 해서 봅시다.
            WarpToPosition(agent.transform.position);
            SetStop(false); //넉백이 끝나면 다시 네비게이션을 시작합니다.
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_lastDestination, 0.3f);
        }
#endif
    }
}
