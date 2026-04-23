using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using Entities;
using Scripts.Combat;
using Scripts.Combat.Projectiles;
using Scripts.SkillSystem;
using UnityEngine;

namespace Work.Code.Skills
{
    public class ReflectSkill : PassiveSkill
    {
        [SerializeField] private LayerMask ownerLayer;
        private VFXComponent _vfxCompo;
        private HealthCompo _healthCompo;
        
        [Inject] private PoolManagerMono _poolManager;

        private Bullet _bullet;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _vfxCompo = _owner.Get<VFXComponent>();
            _healthCompo = _owner.Get<HealthCompo>();
        }

        public override void EnableSkill()
        {
            base.EnableSkill();
            _healthCompo.OnBeforeHit += HandleBeforeHit;
        }

        public override void DisableSkill()
        {
            base.DisableSkill();
            _healthCompo.OnBeforeHit -= HandleBeforeHit;
        }

        private bool HandleBeforeHit(DamageContext context)
        {
            if (context.Source == null)
                return false;
            if (context.Source.TryGetComponent(out Bullet bullet))
            {
                var shooter = bullet.ProjectileShooter;
                var bulletPool = bullet.PoolItem;
                var newBullet = _poolManager.Pop<Bullet>(bulletPool);

                Vector3 reflectDir = -bullet.transform.forward;
                Vector3 position = context.HitPoint + context.HitNormal * 0.05f;
                newBullet.InitBullet(_owner, shooter, position, reflectDir, ownerLayer);
                bullet.PushBullet();
                return true;
            }

            return false;
        }
    }
}