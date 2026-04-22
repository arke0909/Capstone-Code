using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Groups;
using Code.SHS.Entities.Enemies.Targetings.Events;
using Scripts.Entities;
using Scripts.Players;
using SHS.Scripts.Combats.Events;
using SHS.Scripts.Entities.Players;
using SHS.Scripts.NoiseSystems.Events;
using UnityEngine;

namespace Code.SHS.Targetings.Enemies
{
    public class TargetDetector : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<DamagedEvent>,
        ILocalEventSubscriber<NoiseListenedEvent>
    {
        [SerializeField] private StatSO detectionRange;
        [SerializeField] private float closeDetectionRange = 2f;
        [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
        [SerializeField] private LayerMask targetlayerMask;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private Transform eyePoint;

        public ComponentContainer ComponentContainer { get; set; }

        private EntitySensor _sensor;
        private Vector3 EyePosition => eyePoint != null ? eyePoint.position : transform.position + Vector3.up;
        private float DetectionRange => detectionRange.Value;
        private TargetProvider _targetProvider;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            StatOverrideBehavior _statOverrideBehavior = componentContainer.Get<StatOverrideBehavior>();
            detectionRange = _statOverrideBehavior.GetStat(detectionRange);
            _targetProvider = componentContainer.Get<TargetProvider>();
            _sensor = componentContainer.Get<EntitySensor>();
        }

        private void Update()
        {
            if (_targetProvider.CurrentTarget != null)
            {
                if (IsTargetVisible(_targetProvider.CurrentTarget) == false)
                    _targetProvider.SetTarget(null);
                else
                    _targetProvider.SetTarget(_targetProvider.CurrentTarget);
            }
            else
            {
                Entity visibleTarget = TryFindClosestTarget();
                if (visibleTarget != null)
                {
                    _targetProvider.SetTarget(visibleTarget);
                }
            }
        }

        private Entity TryFindClosestTarget()
        {
            float closestDistance = float.MaxValue;
            Entity closestTarget = null;
            foreach (Entity detectedEntity in _sensor.DetectedEntities)
            {
                // 적끼리 싸우게 되면 바뀔듯
                Entity target = detectedEntity.GetComponent<Player>();
                if (target == null)
                    continue;

                if (IsTargetVisible(detectedEntity) == false)
                    continue;

                float distance = Vector3.Distance(EyePosition, target.transform.position);

                if (distance >= closestDistance)
                    continue;

                closestDistance = distance;
                closestTarget = target;
            }

            return closestTarget;
        }

        private bool IsTargetVisible(Entity target)
        {
            if (target == null)
                return false;

            Vector3 playerPos = target.transform.position;
            playerPos.y = EyePosition.y;

            Vector3 direction = playerPos - EyePosition;
            float distance = direction.magnitude;

            // 거리 판별
            {
                if (distance <= closeDetectionRange)
                    return true;
            }
            // 시야각 판별
            {
                Vector3 flatDirection = direction;
                flatDirection.y = 0f;
                if (Vector3.Angle(transform.forward, flatDirection.normalized) > viewAngle * 0.5f)
                    return false;
            }

            // 장애 판별
            {
                if (Physics.Raycast(EyePosition, direction.normalized, distance, obstacleMask))
                {
                    return false;
                }
            }

            return true;
        }

        public void OnLocalEvent(DamagedEvent eventData)
        {
            if (enabled == false) return;
            if (eventData.Dealer is not Player player)
                return;

            _targetProvider.SetTarget(player);
        }

        public void OnLocalEvent(NoiseListenedEvent eventData)
        {
            if (enabled == false) return;
            if (_targetProvider.CurrentTarget != null) return; // 이미 타겟이 있을 때는 소리에 반응하지 않음
            // 소리 볼륨에 따라 반응 조절 필요.

            if (eventData.Source is Enemy enemy && enemy.TargetProvider.CurrentTarget != null)
            {
                _targetProvider.SetTarget(enemy.TargetProvider.CurrentTarget);
                return;
            }

            if (eventData.Source is not Player player)
                return;

            // if (Vector3.Distance(player.transform.position, transform.position) > DetectionRange)
            // return;

            if (IsTargetVisible(player))
                _targetProvider.SetTarget(player);
            else
                _targetProvider.TargetLost(eventData.NoisePosition);
        }


        private void OnDrawGizmosSelected()
        {
            if (detectionRange == null)
                return;

            Vector3 origin = transform.position;
            origin.y = eyePoint != null ? eyePoint.position.y : origin.y + 0.05f;

            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            Gizmos.DrawWireSphere(origin, detectionRange.Value);

            Vector3 leftBoundary = DirFromAngle(-viewAngle * 0.5f) * detectionRange.Value;
            Vector3 rightBoundary = DirFromAngle(viewAngle * 0.5f) * detectionRange.Value;

            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
            Gizmos.DrawLine(origin, origin + leftBoundary);
            Gizmos.DrawLine(origin, origin + rightBoundary);
        }

        private Vector3 DirFromAngle(float angleOffset)
        {
            float angle = transform.eulerAngles.y + angleOffset;
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        }
    }
}