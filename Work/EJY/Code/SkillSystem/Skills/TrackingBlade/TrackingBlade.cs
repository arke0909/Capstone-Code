using System;
using System.Collections.Generic;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.StatusEffectSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;

namespace Code.SkillSystem.Skills.TrackingBlade
{
    public class TrackingBlade : MonoBehaviour, IPoolable
    {
        [SerializeField] private PoolItemSO trackingBladeItemSO;
        [SerializeField] private PoolItemSO trackingBladeHitItemSO;
        [SerializeField] private BuffSO bleedingBuff;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private BuffSO slowBuff;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float delayToRotate = 0.5f;
        [SerializeField] private float lifeTime = 6f;

        public PoolItemSO PoolItem => trackingBladeItemSO;
        public GameObject GameObject => gameObject;

        private Entity _owner;
        private Rigidbody _rigidbody;
        private Pool _myPool;
        private Entity _target;
        private float _currentTime;
        private bool _applySlow;
        private float _additionalRotateSpeed = 0;
        private float _damage;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void SetApplySlow(bool applySlow) => _applySlow = applySlow;

        public void Initialize(Entity owner, Entity target ,Vector3 position, Vector3 direction, float damage)
        {
            trailRenderer?.Clear();

            _owner = owner;
            _target = target;
            transform.position = position;
            transform.forward = direction;
            _damage = damage;
        }

        private void FixedUpdate()
        {
            _currentTime += Time.fixedDeltaTime;

            _additionalRotateSpeed = _currentTime / lifeTime;

            CalcMovement();

            if (_currentTime >= delayToRotate && _target != null && !_target.IsDead)
                RotateToTarget();
        }

        private void CalcMovement()
        {
            _rigidbody.linearVelocity = transform.forward * moveSpeed;
        }

        private void RotateToTarget()
        {
            Vector3 dir = _target.HitTransform.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(dir);
            Quaternion rotation = transform.rotation;

            Quaternion goalRotation = Quaternion.Lerp(rotation, rotationToTarget,Time.fixedDeltaTime * (rotationSpeed + _additionalRotateSpeed * _additionalRotateSpeed));

            transform.rotation = goalRotation;
        }

        private void OnTriggerEnter(Collider other)
        {
            IDamageable damageable = null;
            if (!other.TryGetComponent(out damageable))
                damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitNormal = transform.position - hitPoint;
            if (hitNormal.sqrMagnitude <= 0.0001f)
                hitNormal = -transform.forward;
            else
                hitNormal.Normalize();

            DamageContext context = new DamageContext
            {
                DamageData = new DamageData
                {
                    damage = _damage,
                    defPierceLevel = 1,
                    damageType = DamageType.DOT
                },
                HitPoint = hitPoint,
                HitNormal = hitNormal,
                Source = gameObject,
                Attacker = _owner
            };

            damageable.ApplyDamage(context);

            Entity entity = other.GetComponentInParent<Entity>();
            if (entity != null)
            {
                if (entity.TryGet(out EntityStatusEffect statusEffect))
                {
                    statusEffect.AddStatusEffect(bleedingBuff, this);

                    if (_applySlow)
                    {
                        Debug.Assert(slowBuff != null,
                            $"{nameof(TrackingBlade)} requires {nameof(slowBuff)} when {nameof(_applySlow)} is enabled.", this);

                        if (slowBuff != null)
                            statusEffect.AddStatusEffect(slowBuff, this);
                    }
                }
            }

            Bus.Raise(new PlayEffectEvent(trackingBladeHitItemSO ,transform.position, Quaternion.LookRotation(transform.forward)));
            _myPool.Push(this);
        }

        public void ResetItem()
        {
            _currentTime = 0;
        }
    }
}
