using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace SHS.Scripts.Entities.Players
{
    public class PlayerGunShotAssist : MonoBehaviour, IContainerComponent
    {
        [SerializeField] private float immediateHitRange = 1.25f;
        [SerializeField] private float immediateHitRadius = 0.4f;
        [SerializeField] private int maxHitCount = 8;

        public ComponentContainer ComponentContainer { get; set; }

        private Collider[] _overlapHits;
        private RaycastHit[] _castHits;
        private Entity _owner;
        private int _enemyLayer;
        private int _obstacleLayer;
        private int _hitMask;
#if UNITY_EDITOR
        private bool _hasLastShot;
        private Vector3 _lastFirePosition;
        private Vector3 _lastCastOrigin;
        private Vector3 _lastDirection;
#endif

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _owner = componentContainer.GetSubclassComponent<Entity>();
            _enemyLayer = LayerMask.NameToLayer("Enemy");
            _obstacleLayer = LayerMask.NameToLayer("Obstacle");

            Debug.Assert(_owner != null, "PlayerGunShotAssist: Owner is not assigned.");
            Debug.Assert(_enemyLayer >= 0, "PlayerGunShotAssist: Enemy layer is missing.");
            Debug.Assert(_obstacleLayer >= 0, "PlayerGunShotAssist: Obstacle layer is missing.");

            _hitMask = (1 << _enemyLayer) | (1 << _obstacleLayer);
            _overlapHits = new Collider[maxHitCount];
            _castHits = new RaycastHit[maxHitCount];
        }

        public bool TryGetImmediateHit(Vector3 firePosition, Vector3 direction, out Collider hitCollider,
            out Vector3 hitPoint, out Vector3 hitNormal)
        {
            Debug.Assert(direction.sqrMagnitude > 0.0001f, "PlayerGunShotAssist: Direction is zero.");

            direction.y = 0f;
            direction.Normalize();

            Vector3 castOrigin = _owner.HitTransform.position;
            castOrigin.y = firePosition.y;

#if UNITY_EDITOR
            _hasLastShot = true;
            _lastFirePosition = firePosition;
            _lastCastOrigin = castOrigin;
            _lastDirection = direction;
#endif

            if (TryGetOverlapHit(firePosition, direction, out hitCollider, out hitPoint, out hitNormal))
                return true;

            int hitCount = Physics.SphereCastNonAlloc(castOrigin, immediateHitRadius, direction, _castHits,
                immediateHitRange, _hitMask, QueryTriggerInteraction.Ignore);

            int closestIndex = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _castHits[i];
                if (hit.collider == null || hit.distance >= closestDistance)
                    continue;

                closestIndex = i;
                closestDistance = hit.distance;
            }

            if (closestIndex < 0)
            {
                hitCollider = null;
                hitPoint = Vector3.zero;
                hitNormal = Vector3.zero;
                return false;
            }

            RaycastHit closestHit = _castHits[closestIndex];
            if (closestHit.collider.gameObject.layer == _obstacleLayer)
            {
                hitCollider = null;
                hitPoint = Vector3.zero;
                hitNormal = Vector3.zero;
                return false;
            }

            hitCollider = closestHit.collider;
            hitPoint = closestHit.point;
            hitNormal = closestHit.normal.sqrMagnitude > 0.0001f ? closestHit.normal : -direction;
            return true;
        }

        private bool TryGetOverlapHit(Vector3 firePosition, Vector3 direction, out Collider hitCollider,
            out Vector3 hitPoint, out Vector3 hitNormal)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(firePosition, immediateHitRadius, _overlapHits,
                _hitMask, QueryTriggerInteraction.Ignore);

            Collider closestCollider = null;
            Vector3 closestPoint = Vector3.zero;
            float closestDistanceSqr = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider target = _overlapHits[i];
                if (target == null)
                    continue;

                Vector3 point = target.ClosestPoint(firePosition);
                float distanceSqr = (point - firePosition).sqrMagnitude;
                if (distanceSqr >= closestDistanceSqr)
                    continue;

                closestCollider = target;
                closestPoint = point;
                closestDistanceSqr = distanceSqr;
            }

            if (closestCollider == null || closestCollider.gameObject.layer == _obstacleLayer)
            {
                hitCollider = null;
                hitPoint = Vector3.zero;
                hitNormal = Vector3.zero;
                return false;
            }

            hitCollider = closestCollider;
            hitPoint = closestPoint;

            Vector3 normal = firePosition - closestPoint;
            hitNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : -direction;
            return true;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 direction = _hasLastShot ? _lastDirection : transform.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
                direction = Vector3.forward;
            direction.Normalize();

            Vector3 castOrigin = _hasLastShot ? _lastCastOrigin : transform.position;
            Vector3 firePosition = _hasLastShot ? _lastFirePosition : castOrigin;
            Vector3 castEnd = castOrigin + direction * immediateHitRange;
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized * immediateHitRadius;
            Vector3 up = Vector3.up * immediateHitRadius;

            Gizmos.color = new Color(1f, 0.45f, 0.05f, 0.9f);
            Gizmos.DrawWireSphere(firePosition, immediateHitRadius);

            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.85f);
            Gizmos.DrawWireSphere(castOrigin, immediateHitRadius);
            Gizmos.DrawWireSphere(castEnd, immediateHitRadius);
            Gizmos.DrawLine(castOrigin, castEnd);
            Gizmos.DrawLine(castOrigin + right, castEnd + right);
            Gizmos.DrawLine(castOrigin - right, castEnd - right);
            Gizmos.DrawLine(castOrigin + up, castEnd + up);
            Gizmos.DrawLine(castOrigin - up, castEnd - up);
        }
#endif
    }
}
