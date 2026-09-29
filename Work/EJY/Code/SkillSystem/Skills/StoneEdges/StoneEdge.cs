using System.Collections.Generic;
using System.Threading.Tasks;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.ETC.MapObjects;
using Code.GameEvents;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.Players;
using UnityEngine;

namespace Code.SkillSystem.Skills.StoneEdges
{
    [RequireComponent(typeof(SphereCollider))]
    public class StoneEdge : HittableObject, IPoolable
    {
        [SerializeField] private PoolItemSO hitEffectItem;
        [SerializeField] private PoolItemSO explosionEffectItem;
        [SerializeField] private ParticleSystem effect;
        [SerializeField] private LayerMask whatIsTarget;
        [SerializeField] private float impactDelay = 0.2f;
        [SerializeField] private float stunTime = 3f;
        [SerializeField] private float explosionRadius = 3.5f;
        [SerializeField] private float explosionDamageMultiplier = 1f;
        [SerializeField] private float cameraShakeForce = 1.2f;
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;

        private readonly HashSet<Entity> _hitEntities = new();
        private readonly HashSet<Entity> _explosionHitEntities = new();
        private readonly Collider[] _impactResults = new Collider[32];

        private Entity _owner;
        private DamageCalcCompo _damageCalcCompo;
        private Pool _myPool;
        private SphereCollider _triggerCollider;
        private Vector3 _originSize;
        private float _damage;
        private bool _explodeOnPlayerHit;
        private bool _isReturningToPool;
        private int _lifetimeToken;

        protected override void Awake()
        {
            base.Awake();
            _originSize = transform.localScale;
            _triggerCollider = GetComponent<SphereCollider>();
            DisableParticleCollision();
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _hitEntities.Clear();
            _explosionHitEntities.Clear();
            _owner = null;
            _damageCalcCompo = null;
            _damage = 0f;
            _explodeOnPlayerHit = false;
            _isReturningToPool = false;
            _triggerCollider.enabled = false;
            SetMaxHp(MaxHp);
        }

        public async Task Init(Entity owner, Vector3 position, Vector3 forward, float size, float damage, bool explodeOnPlayerHit)
        {
            _owner = owner;
            _damageCalcCompo = owner.Get<DamageCalcCompo>();
            transform.localScale = _originSize * size;
            transform.position = position;
            transform.forward = forward;
            _damage = damage;
            _explodeOnPlayerHit = explodeOnPlayerHit;
            _hitEntities.Clear();
            _explosionHitEntities.Clear();
            _triggerCollider.enabled = true;
            _isReturningToPool = false;
            SetMaxHp(MaxHp);

            int lifetimeToken = ++_lifetimeToken;

            effect.Play();
            RunImpactAfterDelay(lifetimeToken).Forget();

            await UniTask.WaitForSeconds(effect.main.duration);

            if (_lifetimeToken != lifetimeToken || _isReturningToPool)
                return;

            ReturnToPool();
        }

        public override void ApplyDamage(DamageData damageData, Entity dealer = null)
        {
            if (!_explodeOnPlayerHit || dealer is not Player || _owner == null || _damageCalcCompo == null || _isReturningToPool)
                return;

            base.ApplyDamage(new DamageData
            {
                damage = MaxHp,
                damageType = damageData.damageType,
                defPierceLevel = damageData.defPierceLevel
            }, dealer);
            
        }

        protected override void OnDeath()
        {
            Explode();
        }

        private void OnDrawGizmosSelected()
        {
            SphereCollider sphereCollider = _triggerCollider != null ? _triggerCollider : GetComponent<SphereCollider>();
            if (sphereCollider == null)
                return;

            Vector3 center = transform.TransformPoint(sphereCollider.center);
            Vector3 scale = transform.lossyScale;
            float impactRadius = sphereCollider.radius * Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, impactRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, explosionRadius);
        }
        private async UniTaskVoid RunImpactAfterDelay(int lifetimeToken)
        {
            await UniTask.WaitForSeconds(impactDelay);

            if (_lifetimeToken != lifetimeToken || _isReturningToPool)
                return;

            Vector3 impactCenter = transform.TransformPoint(_triggerCollider.center);
            Vector3 scale = transform.lossyScale;
            float impactRadius = _triggerCollider.radius * Mathf.Max(
                Mathf.Abs(scale.x),
                Mathf.Abs(scale.y),
                Mathf.Abs(scale.z));

            int hitCount = Physics.OverlapSphereNonAlloc(
                impactCenter,
                impactRadius,
                _impactResults,
                whatIsTarget,
                QueryTriggerInteraction.Collide);

            if (hitCount == _impactResults.Length)
                Debug.LogWarning("StoneEdge impact buffer is full.");

            DamageData damageData = _damageCalcCompo.CalculateDamage(_damage, 1f, 1, DamageType.MAGIC);

            for (int i = 0; i < hitCount; i++)
            {
                Collider target = _impactResults[i];
                _impactResults[i] = null;

                Entity entity = target.GetComponentInParent<Entity>();
                if (entity == null || entity == _owner || entity.IsDead || !_hitEntities.Add(entity))
                    continue;

                IDamageable damageable = entity.GetSubclassCompo<IDamageable>();
                if (damageable == null)
                    continue;

                entity.Stun(stunTime);
                damageable.ApplyDamage(damageData, _owner);
                Bus.Raise(new PlayEffectEvent(hitEffectItem, entity.HitTransform.position, Quaternion.identity));
            }
        }

        private void Explode()
        {
            Vector3 explosionCenter = transform.TransformPoint(_triggerCollider.center);
            _triggerCollider.enabled = false;
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            Bus.Raise(new PlayEffectEvent(explosionEffectItem, explosionCenter, Quaternion.identity));
            Bus.Raise(new CameraShakeEvent(transform.position, Vector3.up, cameraShakeForce));
            ApplyExplosionDamage(explosionCenter);
            ReturnToPool();
        }

        private void ApplyExplosionDamage(Vector3 explosionCenter)
        {
            Collider[] targets = Physics.OverlapSphere(
                explosionCenter,
                explosionRadius,
                whatIsTarget,
                QueryTriggerInteraction.Collide);
            DamageData damageData = _damageCalcCompo.CalculateDamage(_damage, explosionDamageMultiplier, 1, DamageType.MAGIC);

            _explosionHitEntities.Clear();

            for (int i = 0; i < targets.Length; i++)
            {
                Collider target = targets[i];
                if (target == null)
                    continue;

                Entity entity = target.GetComponentInParent<Entity>();
                if (entity == null || entity == _owner || entity.IsDead || !_explosionHitEntities.Add(entity))
                    continue;

                IDamageable damageable = entity.GetSubclassCompo<IDamageable>();
                if (damageable == null && !target.TryGetComponent(out damageable))
                    continue;

                Vector3 hitPoint = target.ClosestPoint(explosionCenter);
                Vector3 hitNormal = entity.HitTransform.position - explosionCenter;
                if (hitNormal.sqrMagnitude <= 0.0001f)
                    hitNormal = Vector3.up;
                else
                    hitNormal.Normalize();

                DamageContext context = new DamageContext
                {
                    DamageData = damageData,
                    HitPoint = hitPoint,
                    HitNormal = hitNormal,
                    Source = gameObject,
                    Attacker = _owner
                };

                entity.Stun(stunTime);
                damageable.ApplyDamage(context);
                Bus.Raise(new PlayEffectEvent(hitEffectItem, entity.HitTransform.position, Quaternion.identity));
            }
        }

        private void ReturnToPool()
        {
            if (_isReturningToPool || _myPool == null)
                return;

            _isReturningToPool = true;
            _lifetimeToken++;
            _myPool.Push(this);
        }

        private void DisableParticleCollision()
        {
            var collision = effect.collision;
            collision.enabled = false;
        }
    }
}


