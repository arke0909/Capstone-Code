using System;
using System.Collections.Generic;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using UnityEngine;
using Code.Combat;
using UnityEngine.Serialization;

namespace Code.SkillSystem.Skills.GravityFields
{
    public class GravityField : MonoBehaviour, IPoolable
    {
        [SerializeField] private LayerMask whatIsEnemy;
        [SerializeField] private PoolItemSO gravityFieldPoolItem;
        [SerializeField] private StatSO moveSpeedStatSO;
        [SerializeField] private int maxTargetCount;
        [SerializeField] private float sizeChangeDuration = 0.25f;
        [SerializeField] private float initExpansionSize = 2f;
        [SerializeField] private float remainTime = 5f;
        [SerializeField] private float delayToStun = 1.25f;
        [SerializeField] private float stunTime = 1f;
        [Range(0.01f, 0.99f), SerializeField] private float decreasePercent = 0.8f;

        private Dictionary<Entity, float> _stunEntities;

        private float _currentTime;
        private float _endTime;
        private bool _isSlowEntity;
        private bool _isStunEntity;

        public PoolItemSO PoolItem => gravityFieldPoolItem;
        public GameObject GameObject => gameObject;

        private Pool _myPool;
        private Vector3 _originSize;

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        private void Awake()
        {
            _originSize = transform.localScale;
            _endTime = remainTime + sizeChangeDuration;
            _stunEntities = new Dictionary<Entity, float>();
        }

        public void Init(Vector3 position, bool isSlowEntity, bool isStunEntity)
        {
            transform.position = position;
            _isSlowEntity = isSlowEntity;
            _isStunEntity = isStunEntity;

            _currentTime = 0f;
            transform.localScale = Vector3.zero;

            _stunEntities.Clear();
        }

        private void Update()
        {
            _currentTime += Time.deltaTime;

            ChangeSize();

            if (_isStunEntity)
            {
                var keys = new List<Entity>(_stunEntities.Keys);

                foreach (var entity in keys)
                {
                    if (entity == null)
                    {
                        _stunEntities.Remove(entity);
                        continue;
                    }

                    _stunEntities[entity] += Time.deltaTime;

                    if (_stunEntities[entity] >= delayToStun)
                    {
                        entity.Stun(stunTime);
                        _stunEntities.Remove(entity);
                    }
                }
            }
            
            if (_currentTime >= _endTime)
            {
                _myPool.Push(this);
            }
        }
        
        private void SetLerpSize(float a, float b, float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            float size = Mathf.Lerp(a, b, ratio);
            transform.localScale = _originSize * size;
        }

        private void ChangeSize()
        {
            float ratio;
            
            if (_currentTime < sizeChangeDuration)
            {
                ratio = _currentTime / sizeChangeDuration;
                SetLerpSize(0, initExpansionSize, ratio);
            }
            else if (_currentTime - Time.deltaTime < sizeChangeDuration)
            {
                transform.localScale = _originSize * initExpansionSize;
                return;
            }

            if (_currentTime > remainTime)
            {
                ratio = (_currentTime - remainTime) / sizeChangeDuration;
                SetLerpSize(initExpansionSize, 0, ratio);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Bullet bullet))
            {
                bullet.SetVelocity(1 - decreasePercent);
                return;
            }

            if (_isSlowEntity && other.TryGetComponent(out Entity entity)
                               && entity.TryGet(out StatOverrideBehavior statCompo))
            {
                var targetStat = statCompo.GetStat(moveSpeedStatSO);
                targetStat.AddPercentModifier(this, -decreasePercent);

                if (_isStunEntity && !_stunEntities.ContainsKey(entity))
                {
                    _stunEntities.Add(entity, 0f);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Bullet bullet))
            {
                bullet.SetVelocity(1);
                return;
            }

            if (_isSlowEntity && other.TryGetComponent(out Entity entity)
                               && entity.TryGet(out StatOverrideBehavior statCompo))
            {
                var targetStat = statCompo.GetStat(moveSpeedStatSO);
                targetStat.RemovePercentModifier(this);

                if (_isStunEntity)
                {
                    _stunEntities.Remove(entity);
                }
            }
        }

        public void ResetItem()
        {
            _stunEntities.Clear();
            transform.localScale = _originSize;
        }
    }
}