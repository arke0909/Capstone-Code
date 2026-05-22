using System.Collections.Generic;
using System.Threading.Tasks;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Cysharp.Threading.Tasks;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Code.SkillSystem.Skills.StoneEdges
{
    public class StoneEdge : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolItemSO hitEffectItem;
        [SerializeField] private ParticleSystem effect;
        [SerializeField] private float impactDelay = 0.2f;
        [SerializeField] private float impactDuration = 0.8f;
        [SerializeField] private float stunTime = 3f;
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;

        private readonly HashSet<Entity> _hitEntities = new HashSet<Entity>();

        private Entity _owner;
        private DamageCalcCompo _damageCalcCompo;
        private Pool _myPool;
        private Rigidbody _triggerRigidbody;
        private SphereCollider _triggerCollider;
        private Vector3 _originSize;
        private float _impactStartTime;
        private float _impactEndTime;

        private void Awake()
        {
            _originSize = transform.localScale;
            ConfigureTrigger();
            DisableParticleCollision();
        }

        private void Update()
        {
            if (_triggerCollider == null || !_triggerCollider.enabled)
                return;

            if (Time.time > _impactEndTime)
                _triggerCollider.enabled = false;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _hitEntities.Clear();
            _owner = null;
            _damageCalcCompo = null;
            _impactStartTime = float.MaxValue;
            _impactEndTime = 0f;

            if (_triggerCollider != null)
                _triggerCollider.enabled = false;
        }

        public async Task Init(Entity owner, Vector3 position, Vector3 forward, float size)
        {
            _owner = owner;
            _damageCalcCompo = owner.Get<DamageCalcCompo>();
            transform.localScale = _originSize * size;
            transform.position = position;
            transform.forward = forward;
            _hitEntities.Clear();
            _impactStartTime = Time.time + impactDelay;
            _impactEndTime = _impactStartTime + impactDuration;

            if (_triggerCollider != null)
                _triggerCollider.enabled = true;

            effect.Play();

            await UniTask.WaitForSeconds(effect.main.duration);

            _myPool.Push(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryApplyImpact(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryApplyImpact(other);
        }

        private void TryApplyImpact(Collider other)
        {
            if (_owner == null || _damageCalcCompo == null)
                return;

            if (Time.time < _impactStartTime || Time.time > _impactEndTime)
                return;

            Entity entity = other.GetComponentInParent<Entity>();

            if (entity == null || entity == _owner || entity.IsDead)
                return;

            IDamageable damageable = entity.GetSubclassCompo<IDamageable>();
            bool canApplyDamage = damageable != null;

            if ((!canApplyDamage) || !_hitEntities.Add(entity))
                return;

            entity.Stun(stunTime);

            DamageData damageData = _damageCalcCompo.CalculateDamage(10, 1, 1, DamageType.MAGIC);
            damageable.ApplyDamage(damageData, _owner);

            Bus.Raise(new PlayEffectEvent(hitEffectItem, entity.HitTransform.position, Quaternion.identity));
        }

        private void ConfigureTrigger()
        {
            if (!TryGetComponent(out _triggerRigidbody))
                _triggerRigidbody = gameObject.AddComponent<Rigidbody>();

            _triggerRigidbody.useGravity = false;
            _triggerRigidbody.isKinematic = true;
            _triggerRigidbody.interpolation = RigidbodyInterpolation.None;
            _triggerRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

            if (!TryGetComponent(out _triggerCollider))
                _triggerCollider = gameObject.AddComponent<SphereCollider>();

            _triggerCollider.isTrigger = true;
            _triggerCollider.enabled = false;
        }

        private void DisableParticleCollision()
        {
            var collision = effect.collision;
            collision.enabled = false;
        }
    }
}