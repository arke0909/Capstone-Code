﻿using Chipmunk.ComponentContainers;
using Code.StatusEffectSystem;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Areas;
using Scripts.Combat.Datas;
using Scripts.Effects;
using Scripts.Entities;
using System;
using UnityEngine;

namespace Code.SkillSystem.Skills.Bombing
{
    public class BombingMissile : MonoBehaviour, IPoolable
    {
        [SerializeField] private LayerMask whatIsTarget;
        [SerializeField] private PoolManagerSO poolManagerSO;
        [SerializeField] private PoolItemSO bombingItemSO;
        [SerializeField] private PoolItemSO bombEffectItemSO;
        [SerializeField] private PoolItemSO floorItemSO;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private BuffSO slowAndAdditionalDamageData;
        [field: SerializeField] public bool CreateFloor { get; set; }
        [field: SerializeField] public bool SlowAndAdditionalDamage { get; set; }
        public PoolItemSO PoolItem => bombingItemSO;
        public GameObject GameObject => gameObject;

        private Pool _myPool;
        private DamageData _currentDamageData;
        private Entity _owner;


        public event Action OnPush;

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void SetDamageData(DamageData damageData) => _currentDamageData = damageData;
        public void SetOwner(Entity owner) => _owner = owner;

        private void OnTriggerEnter(Collider other)
        {
            damageCaster.CastDamage(_currentDamageData, damageCaster.transform.position, -transform.up, null);

            PoolingEffect poolingEffect = poolManagerSO.Pop(bombEffectItemSO) as PoolingEffect;
            poolingEffect.PlayVFX(damageCaster.transform.position, Quaternion.identity);

            if (SlowAndAdditionalDamage)
                ApplySlowAndAdditionalDamage();

            if (CreateFloor)
            {
                DealingArea floor = poolManagerSO.Pop(floorItemSO) as DealingArea;
                floor.Init(_owner, damageCaster.transform.position);
            }

            OnPush?.Invoke();
            _myPool.Push(this);
        }

        public void ApplySlowAndAdditionalDamage()
        {
            Collider[] targets = new Collider[10];

            Vector3 overlapPos = transform.position;

            Physics.Raycast(overlapPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, whatIsTarget);

            overlapPos.y = hit.point.y;

            // temp
            int count = Physics.OverlapSphereNonAlloc(overlapPos, 4, targets, whatIsTarget);

            for (int i = 0; i < count; i++)
            {
                var compoContainer = targets[i].gameObject.GetComponent<ComponentContainer>();
                var entityStatusEffect = compoContainer.Get<EntityStatusEffect>();

                entityStatusEffect.AddStatusEffect(slowAndAdditionalDamageData.GetStatusEffectInfo());
            }
        }

        public void ResetItem()
        {
            damageCaster.InitCaster(_owner);
        }
    }
}