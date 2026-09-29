using Chipmunk.ComponentContainers;
using Scripts.Entities;
using SHS.Scripts.Crosshairs;
using UnityEngine;

namespace Code.Players
{
    public class TargetingComponent : MonoBehaviour, IContainerComponent
    {
        [Header("Targeting")] [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float targetSearchRadius = 2.5f;

        public ComponentContainer ComponentContainer { get; set; }

        private CrosshairBehavior _crosshairBehavior;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            ComponentContainer = componentContainer;
            _crosshairBehavior = componentContainer.Get<CrosshairBehavior>();
        }

        public bool TryResolveSoftTarget(out Transform target, out Vector3 point)
        {
            Camera mainCamera = Camera.main;
            Ray aimRay = mainCamera.ScreenPointToRay(_crosshairBehavior.GetCrosshairScreenPosition());

            point = _crosshairBehavior.GetWorldAimPosition();

            if (TryGetTargetRayHit(aimRay, out RaycastHit hit))
            {
                point = hit.point;

                if (IsTargetLayer(hit.collider.gameObject.layer))
                {
                    Entity entity = hit.collider.GetComponentInParent<Entity>();
                    if (entity != null)
                    {
                        target = entity.transform;
                        point = entity.HitTransform.position;
                        return true;
                    }
                }
            }

            return TryFindNearestTarget(point, mainCamera.transform.position, out target, out point);
        }

        private bool TryGetTargetRayHit(Ray aimRay, out RaycastHit hit)
        {
            int mask = targetLayer | obstacleLayer;
            return Physics.Raycast(aimRay, out hit, Camera.main.farClipPlane, mask);
        }

        private bool TryFindNearestTarget(Vector3 point, Vector3 cameraPosition, out Transform target, out Vector3 targetPoint)
        {
            Collider[] hits = Physics.OverlapSphere(point, targetSearchRadius, targetLayer);

            target = null;
            targetPoint = point;
            float bestDistanceSqr = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                Entity entity = hit.GetComponentInParent<Entity>();
                if (entity == null)
                    continue;

                Transform candidate = entity.transform;
                Vector3 candidatePoint = entity.HitTransform.position;

                if (HasBlockingObstacle(cameraPosition, candidatePoint))
                    continue;

                Vector3 comparePoint = candidatePoint;
                comparePoint.y = point.y;

                float distanceSqr = (comparePoint - point).sqrMagnitude;
                if (distanceSqr >= bestDistanceSqr)
                    continue;

                bestDistanceSqr = distanceSqr;
                target = candidate;
                targetPoint = candidatePoint;
            }

            return target != null;
        }

        private bool HasBlockingObstacle(Vector3 origin, Vector3 targetPoint)
        {
            Vector3 direction = targetPoint - origin;
            float distance = direction.magnitude;

            if (distance <= 0.001f)
                return false;

            return Physics.Raycast(origin, direction / distance, distance, obstacleLayer);
        }

        private bool IsTargetLayer(int layer)
        {
            return (targetLayer.value & (1 << layer)) != 0;
        }
    }
}
