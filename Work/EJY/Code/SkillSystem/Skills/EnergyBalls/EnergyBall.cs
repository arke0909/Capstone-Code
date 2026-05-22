using System;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Combat.Projectiles;
using Scripts.Entities;
using SHS.Scripts;
using Unity.Mathematics;
using UnityEngine;

namespace Code.SkillSystem.Skills.EnergyBalls
{
    public class EnergyBall : MonoBehaviour, IProjectile
    {
        [SerializeField] private PoolItemSO energyBallItem;
        [SerializeField] private PoolItemSO energyBallEffectItem;
        [SerializeField] private PoolItemSO floorItem;
        [SerializeField] private OverlapDamageCaster damageCaster;
        [SerializeField] private ParticleSystem trailParticle;
        [SerializeField] private PoolManagerSO poolManager;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private SoundID soundID;

        public PoolItemSO PoolItem => energyBallItem;
        public GameObject GameObject => gameObject;

        private Action<int> _OnHit;
        private Entity _owner;
        private Pool _myPool;
        private DamageCalcCompo _damageCalcCompo;
        private IProjectileShooter _projectileShooter;
        private float _explosionRange;
        private bool _isCreateFloor;

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public void ResetItem()
        {
        }

        public void InitProjectile(Entity owner, IProjectileShooter projectileShooter, Vector3 initPos,
            Vector3 direction,
            LayerMask excludeLayer)
        {
            trailParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            trailParticle.Clear(true);

            _owner = owner;
            _projectileShooter = projectileShooter;
            _damageCalcCompo = owner.Get<DamageCalcCompo>();
            damageCaster.InitCaster(owner);
            transform.position = initPos;
            transform.forward = direction.normalized;
            GetComponent<Rigidbody>().linearVelocity = transform.forward * projectileShooter.ProjectileSpeed;
            GetComponent<Collider>().excludeLayers = excludeLayer;

            trailParticle.Play();
        }

        public void AdditionalInit(float explosionRange, Action<int> onEnergyHit, bool isCreateFloor)
        {
            _explosionRange = explosionRange;
            damageCaster.SetRadius(_explosionRange);

            _OnHit = onEnergyHit;

            _isCreateFloor = isCreateFloor;
        }

        private void OnTriggerEnter(Collider other)
        {
            BroAudio.Play(soundID, transform.position);

            
            DamageData damageData = _damageCalcCompo.CalculateDamage
            (
                _projectileShooter.DefaultDamage,
                _projectileShooter.DamageMultiplier,
                _projectileShooter.DefPierceLevel,
                DamageType.RANGE
            );
            
            int hitEnemy = damageCaster.CastDamage(damageData, transform.position, transform.forward, null);
            _OnHit?.Invoke(hitEnemy);
            
            _myPool.Push(this);
            
            Bus.Raise(new PlayEffectEvent(energyBallEffectItem, transform.position, Quaternion.LookRotation(transform.forward), _explosionRange));
            
            if(_isCreateFloor)
            {
                RemnantFloor rf = poolManager.Pop(floorItem) as RemnantFloor;

                Physics.Raycast(transform.position, Vector3.down, out var hit,5f,whatIsGround);
                Vector3 groundPos = hit.point;
                groundPos.y += 0.3f;
                rf?.Init(_owner, groundPos);
            }
        }
    }
}