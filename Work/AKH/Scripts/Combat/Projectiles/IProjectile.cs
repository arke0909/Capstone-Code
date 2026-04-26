using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using SHS.Scripts;
using UnityEngine;

namespace Scripts.Combat.Projectiles
{
    public interface IProjectile : IPoolable
    {
        void InitProjectile(Entity owner, IProjectileShooter projectileShooter, Vector3 initPos, Vector3 direction, LayerMask excludeLayer);
    }
}
