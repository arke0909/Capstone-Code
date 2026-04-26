using Code.SHS.Entities.Enemies.Combat;
using Cysharp.Threading.Tasks;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.SkillSystem.Skills.Grab
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class GrabHookProjectile : MonoBehaviour
    {
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private SphereCollider triggerCollider;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private float speed = 35f;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private MovementDataSO pullMovementData;
        [SerializeField] private float pullStopDistance = 1.2f;
        [SerializeField] private float controlLockDuration = 1f;

        private const float DefaultColliderRadius = 0.25f;

        private Entity _owner;
        private Transform _pullAnchor;
        private Vector3 _direction;
        private float _traveledDistance;
        private Vector3 _previousPosition;
        private bool _initialized;
        private bool _resolved;
        private int _launchId;

        private MovementDataSO _fallbackPullMovementData;
        private DamageData _damageData;

        public LayerMask HitMask
        {
            get => hitMask;
            set => hitMask = value;
        }

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0.01f, value);
        }

        public float MaxDistance
        {
            get => maxDistance;
            set => maxDistance = Mathf.Max(0.1f, value);
        }

        public float LifeTime
        {
            get => lifeTime;
            set => lifeTime = Mathf.Max(0.05f, value);
        }

        public MovementDataSO PullMovementData
        {
            get => pullMovementData;
            set => pullMovementData = value;
        }

        public float PullStopDistance
        {
            get => pullStopDistance;
            set => pullStopDistance = Mathf.Max(0f, value);
        }

        public float ControlLockDuration
        {
            get => controlLockDuration;
            set => controlLockDuration = Mathf.Max(0f, value);
        }

        private void FixedUpdate()
        {
            if (!_initialized || _resolved)
                return;

            Vector3 currentPosition = transform.position;
            _traveledDistance += Vector3.Distance(_previousPosition, currentPosition);
            _previousPosition = currentPosition;

            if (_traveledDistance >= maxDistance)
            {
                DestroyProjectile();
            }
        }

        public void Launch(
            Entity owner,
            Transform pullAnchor,
            Vector3 direction,
            DamageData damageData)
        {
            _owner = owner;
            _pullAnchor = pullAnchor;
            _direction = direction.normalized;
            _damageData = damageData;
            _traveledDistance = 0f;
            _previousPosition = transform.position;
            _initialized = true;
            _resolved = false;
            _launchId++;

            if (_direction.sqrMagnitude < 0.0001f)
                _direction = transform.forward;

            transform.forward = _direction;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.linearVelocity = _direction * speed;
            }

            trail?.Clear();

            ExpireAfterLifetimeAsync(_launchId).Forget();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_initialized || _resolved || other == null)
                return;

            if (triggerInteraction == QueryTriggerInteraction.Ignore && other.isTrigger)
                return;

            if (!IsLayerIncluded(other.gameObject.layer))
                return;

            if (!TryResolveHit(other, out Entity targetEntity, out Vector3 hitPoint, out Vector3 hitNormal))
                return;

            _resolved = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (targetEntity != null)
            {
                HandleEntityHit(other, targetEntity, hitPoint, hitNormal);
            }

            DestroyProjectile();
        }

        private bool TryResolveHit(Collider other, out Entity targetEntity, out Vector3 hitPoint, out Vector3 hitNormal)
        {
            targetEntity = other.GetComponentInParent<Entity>();
            hitPoint = Vector3.zero;
            hitNormal = -transform.forward;

            if (_owner != null && targetEntity == _owner)
                return false;

            ResolveHitInfo(other, out hitPoint, out hitNormal);
            return true;
        }

        private void HandleEntityHit(Collider other, Entity targetEntity, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (TryResolveDamageable(other, targetEntity, out IDamageable damageable))
            {
                DamageContext context = new DamageContext
                {
                    DamageData = _damageData,
                    HitPoint = hitPoint,
                    HitNormal = hitNormal,
                    Source = gameObject,
                    Attacker = _owner
                };

                damageable.ApplyDamage(context);
                _owner?.OnAttack?.Invoke(_owner, damageable);
            }

            PullTargetAsync(targetEntity).Forget();
        }

        private bool TryResolveDamageable(Collider other, Entity targetEntity, out IDamageable damageable)
        {
            damageable = null;

            if (other.TryGetComponent(out damageable))
                return true;

            return targetEntity != null && targetEntity.TryGetComponent(out damageable);
        }

        private void ResolveHitInfo(Collider other, out Vector3 point, out Vector3 normal)
        {
            Vector3 referencePoint = _previousPosition;
            point = other.ClosestPoint(referencePoint);

            if ((point - referencePoint).sqrMagnitude < 0.0001f)
                point = other.ClosestPoint(transform.position);

            normal = referencePoint - point;
            if (normal.sqrMagnitude < 0.0001f)
                normal = -transform.forward;
            else
                normal.Normalize();
        }

        private async UniTaskVoid ExpireAfterLifetimeAsync(int launchId)
        {
            try
            {
                await UniTask.WaitForSeconds(lifeTime, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            catch
            {
                return;
            }

            if (this == null || !_initialized || _resolved || launchId != _launchId)
                return;

            DestroyProjectile();
        }

        private async UniTaskVoid PullTargetAsync(Entity targetEntity)
        {
            MovementDataSO movementData = GetPullMovementData();
            if (movementData == null || targetEntity == null)
                return;

            ISkillMovement skillMovement = targetEntity.GetComponent<ISkillMovement>();
            skillMovement ??= targetEntity.GetComponentInChildren<ISkillMovement>();

            if (skillMovement == null)
                return;

            float lockDuration = Mathf.Max(controlLockDuration, movementData.duration);
            ApplyControlLock(targetEntity, lockDuration);

            Vector3 anchorPos = _pullAnchor != null
                ? _pullAnchor.position
                : _owner != null
                    ? _owner.transform.position
                    : targetEntity.transform.position;

            Vector3 pullDirection = anchorPos - targetEntity.transform.position;
            pullDirection.y = 0f;

            if (pullDirection.sqrMagnitude < 0.0001f)
                return;

            float distanceToAnchor = pullDirection.magnitude;
            if (distanceToAnchor <= pullStopDistance)
                return;

            pullDirection /= distanceToAnchor;

            bool prevCanMove = skillMovement.CanMove;
            skillMovement.CanMove = false;
            skillMovement.SetRotation(pullDirection);
            skillMovement.ApplyMovementData(pullDirection, movementData);

            await UniTask.WaitForSeconds(movementData.duration);

            if (skillMovement is Component movementComponent && movementComponent != null)
            {
                skillMovement.CanMove = prevCanMove;
            }
        }

        private static void ApplyControlLock(Entity targetEntity, float duration)
        {
            if (targetEntity is IStunable stunable)
            {
                stunable.Stun(duration);
            }
        }

        private void DestroyProjectile()
        {
            _resolved = true;
            _initialized = false;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Destroy(gameObject);
        }

        private bool IsLayerIncluded(int layer)
        {
            return (hitMask.value & (1 << layer)) != 0;
        }

        private MovementDataSO GetPullMovementData()
        {
            if (pullMovementData != null)
                return pullMovementData;

            if (_fallbackPullMovementData == null)
            {
                _fallbackPullMovementData = ScriptableObject.CreateInstance<MovementDataSO>();
                _fallbackPullMovementData.maxSpeed = 22f;
                _fallbackPullMovementData.duration = 0.45f;
                _fallbackPullMovementData.moveCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
            }

            return _fallbackPullMovementData;
        }
    }
}
