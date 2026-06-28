using System;
using System.Collections;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using UnityEngine.AI;

namespace SHS.Scripts.Skills.Miner
{
    public class CaveInRock : MonoBehaviour, IPoolable, IDamageable
    {
        [Header("Required")]
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        [SerializeField] private Collider bodyCollider;
        [SerializeField] private NavMeshObstacle navMeshObstacle;
        [SerializeField] private LayerMask targetLayer;

        [Header("Fall")]
        [SerializeField] private float fallHeight = 10f;
        [SerializeField] private float fallDuration = 0.65f;
        [SerializeField] private float landingRadius = 1.3f;
        [SerializeField] private float landingDamage = 8f;

        [Header("Explosion")]
        [SerializeField] private float explosionDamageThreshold = 10f;
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float explosionDamage = 14f;

        [Header("Damage")]
        [SerializeField] private int defPierceLevel = 1;
        [SerializeField] private int maxTargetCount = 12;

        public GameObject GameObject => gameObject;
        public event Action<float> OnTakeDamage;

        private readonly HashSet<Entity> _hitEntities = new();

        private Pool _myPool;
        private Entity _owner;
        private DamageCalcCompo _damageCalcCompo;
        private Collider[] _targetColliders;
        private Coroutine _fallCoroutine;
        private float _receivedDamage;
        private bool _canExplode;
        private bool _isReturning;

        private void Awake()
        {
            if (PoolItem == null)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires {nameof(PoolItem)}.");

            if (bodyCollider == null)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires {nameof(bodyCollider)}.");

            if (navMeshObstacle == null)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires {nameof(navMeshObstacle)}.");

            if (targetLayer.value == 0)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires {nameof(targetLayer)}.");

            if (maxTargetCount <= 0)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires positive {nameof(maxTargetCount)}.");

            _targetColliders = new Collider[maxTargetCount];
            bodyCollider.enabled = false;
            navMeshObstacle.enabled = false;
        }

        public void Init(Entity owner, Vector3 landingPosition)
        {
            if (owner == null)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires owner.");

            _owner = owner;
            _damageCalcCompo = owner.Get<DamageCalcCompo>();

            if (_damageCalcCompo == null)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires owner {nameof(DamageCalcCompo)}.");

            transform.position = landingPosition + Vector3.up * fallHeight;
            bodyCollider.enabled = false;
            navMeshObstacle.enabled = false;
            _canExplode = false;
            _isReturning = false;
            _receivedDamage = 0f;

            _fallCoroutine = StartCoroutine(FallRoutine(landingPosition));
        }

        public void Explode(Entity attacker)
        {
            if (!_canExplode || _isReturning)
                return;

            _canExplode = false;
            _isReturning = true;
            bodyCollider.enabled = false;
            navMeshObstacle.enabled = false;

            ApplyAreaDamage(explosionDamage, explosionRadius, DamageType.MAGIC, attacker);
            ReturnToPool();
        }

        public void ApplyDamage(DamageData damageData, Entity dealer = null)
        {
            if (!_canExplode || _isReturning)
                return;

            _receivedDamage += damageData.damage;
            OnTakeDamage?.Invoke(damageData.damage);

            if (_receivedDamage >= explosionDamageThreshold)
                Explode(dealer);
        }

        public void ApplyDamage(DamageContext context)
        {
            ApplyDamage(context.DamageData, context.Attacker);
        }

        private IEnumerator FallRoutine(Vector3 landingPosition)
        {
            Vector3 startPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fallDuration);
                transform.position = Vector3.Lerp(startPosition, landingPosition, t);
                yield return null;
            }

            transform.position = landingPosition;
            bodyCollider.enabled = true;
            navMeshObstacle.enabled = true;
            _canExplode = true;
            _fallCoroutine = null;

            ApplyAreaDamage(landingDamage, landingRadius, DamageType.MELEE, _owner);
        }

        private void ApplyAreaDamage(float damage, float radius, DamageType damageType, Entity attacker)
        {
            _hitEntities.Clear();

            DamageData damageData = _damageCalcCompo.CalculateDamage(
                damage,
                1f,
                defPierceLevel,
                damageType);

            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                _targetColliders,
                targetLayer,
                QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Collider targetCollider = _targetColliders[i];
                Entity entity = targetCollider.GetComponentInParent<Entity>();

                if (entity == null || entity == _owner || entity.IsDead)
                    continue;

                if (!_hitEntities.Add(entity))
                    continue;

                IDamageable damageable = entity.GetSubclassCompo<IDamageable>();

                if (damageable == null)
                    continue;

                Vector3 hitPoint = targetCollider.ClosestPoint(transform.position);
                Vector3 hitNormal = hitPoint - transform.position;

                if (hitNormal.sqrMagnitude < 0.0001f)
                    hitNormal = Vector3.up;

                damageable.ApplyDamage(new DamageContext
                {
                    DamageData = damageData,
                    HitPoint = hitPoint,
                    HitNormal = hitNormal.normalized,
                    Source = gameObject,
                    Attacker = attacker
                });
            }
        }

        private void ReturnToPool()
        {
            if (_myPool == null)
                throw new InvalidOperationException($"{nameof(CaveInRock)} requires pool setup.");

            _myPool.Push(this);
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            if (_fallCoroutine != null)
            {
                StopCoroutine(_fallCoroutine);
                _fallCoroutine = null;
            }

            _owner = null;
            _damageCalcCompo = null;
            _canExplode = false;
            _isReturning = false;
            _receivedDamage = 0f;
            _hitEntities.Clear();

            if (bodyCollider != null)
                bodyCollider.enabled = false;

            if (navMeshObstacle != null)
                navMeshObstacle.enabled = false;
        }

        private void OnValidate()
        {
            Debug.Assert(PoolItem != null, $"{nameof(CaveInRock)} requires {nameof(PoolItem)}.", this);
            Debug.Assert(bodyCollider != null, $"{nameof(CaveInRock)} requires {nameof(bodyCollider)}.", this);
            Debug.Assert(navMeshObstacle != null, $"{nameof(CaveInRock)} requires {nameof(navMeshObstacle)}.", this);
            Debug.Assert(targetLayer.value != 0, $"{nameof(CaveInRock)} requires {nameof(targetLayer)}.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, landingRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
