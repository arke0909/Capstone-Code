using System;
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
        [SerializeField] private StatusEffectCreateData slowStatusEffectCreateData;
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float delayToRotate = 0.5f;
        [SerializeField] private float lifeTime = 6f;
        [SerializeField] private float additionalRotationSpeedMultiplier = 30f;

        public PoolItemSO PoolItem => trackingBladeItemSO;
        public GameObject GameObject => gameObject;

        private Entity _owner;
        private Rigidbody _rigidbody;
        private Pool _myPool;
        private Entity _target;
        private float _currentTime;
        private bool _applySlow;
        private float _additionalRotateSpeed = 0;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }
        
        public void SetApplySlow(bool applySlow) => _applySlow = applySlow;

        public void Initialize(Entity owner, Entity target ,Vector3 position, Vector3 direction)
        {
            trailRenderer?.Clear();
            
            _owner = owner;
            _target = target;
            transform.position = position;
            transform.forward = direction;
        }

        private void FixedUpdate()
        {
            _currentTime += Time.fixedDeltaTime;
            
            _additionalRotateSpeed = _currentTime / lifeTime;
            
            CalcMovement();
            
            if(_currentTime >= delayToRotate && _target != null && !_target.IsDead)
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
            if (other.TryGetComponent(out Entity entity))
            {
                if(entity.TryGetComponent(out IDamageable damageable))
                    damageable.ApplyDamage(new DamageData
                    {
                        damage = 3,
                        defPierceLevel = 1,
                        damageType = DamageType.DOT
                    },
                    _owner);

                var bleedingBuffInfos = bleedingBuff.GetStatusEffectInfo();
                
                if(_applySlow)
                    bleedingBuffInfos.Add(new StatusEffectInfo(bleedingBuff, slowStatusEffectCreateData));

                if (entity.TryGet(out EntityStatusEffect statusEffect))
                {
                    statusEffect.AddStatusEffect(bleedingBuffInfos);
                }
                    
                
                Bus.Raise(new PlayEffectEvent(trackingBladeHitItemSO ,transform.position, Quaternion.LookRotation(transform.forward)));
                _myPool.Push(this);
            }
        }

        public void ResetItem()
        {
            _currentTime = 0;
        }
    }
}