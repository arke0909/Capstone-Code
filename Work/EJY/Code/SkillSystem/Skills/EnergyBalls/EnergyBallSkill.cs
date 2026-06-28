using System;
using Ami.BroAudio;
using Chipmunk.ComponentContainers;
using Code.ETC;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Entities;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Manage;
using SHS.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.SkillSystem.Skills.EnergyBalls
{
    public class EnergyBallSkill : ActiveSkill, IProjectileShooter
    {
        [SerializeField] private Transform firePos;
        [SerializeField] private LayerMask excludeLayer;
        [SerializeField] private PoolItemSO energyBallPoolItem;
        [SerializeField] private SoundID energyBallSoundID;
        [SerializeField] private float damage = 7f;
        [SerializeField] private float projectileMaxRange = 25f;
        [SerializeField] private float explosionRange = 2.5f;
        [SerializeField] private float additionalExplosionRange = 0.8f;
        [SerializeField] private float projectileSpeed = 25f;
        [SerializeField] private float onHitCooldown = 0.8f;

        [Inject] private PoolManagerMono _poolManager;
        private IAimProvider _aimProvider;
        private VFXComponent _vfxCompo;
        private bool _createFloor;
        
        private Action<int> OnEnergyBallHit;
        
        public float DefaultDamage => damage;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileMaxRange => projectileMaxRange;
        public float DamageMultiplier => 1f;
        public int DefPierceLevel => 1;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _vfxCompo = container.Get<VFXComponent>();
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
        }

        private void UpgradeExplosionRange()
        {
            explosionRange += additionalExplosionRange;
        }

        private void RollbackExplosionRange()
        {
            explosionRange -= additionalExplosionRange;
        }
        
        private void UpgradeOnHitCooldown()
        {
            OnEnergyBallHit += CooldownOnHit;
            _createFloor = true;
        }

        private void RollbackOnHitCooldown()
        {
            OnEnergyBallHit -= CooldownOnHit;
            _createFloor = false;
        }

        private void CooldownOnHit(int hitCount)
        {
            float cooldownAmount = hitCount * onHitCooldown;

            var skillSocket = _container.Get<ActiveSkillComponent>().GetSocket(this) as ActiveSkillSocket;
            
            skillSocket?.ReduceCooldown(cooldownAmount);
        }
        
        public override void OnSkillTrigger()
        {
            _vfxCompo.PlayVFX("EnergyBallMuzzle", firePos.position, Quaternion.identity);

            Vector3 aimPoint = _aimProvider.GetAimPosition(firePos.position.y);
            Vector3 direction = aimPoint - firePos.position;

            BroAudio.Play(energyBallSoundID, aimPoint);
            _owner.RotateToTarget(aimPoint);
            
            EnergyBall energyBall = _poolManager.Pop<EnergyBall>(energyBallPoolItem);
            energyBall.InitProjectile(_owner, this, firePos.position, direction, excludeLayer);
            energyBall.AdditionalInit(explosionRange, OnEnergyBallHit, _createFloor);
        }
    }
}
