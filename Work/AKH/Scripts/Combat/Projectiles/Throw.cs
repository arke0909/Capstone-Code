using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using SHS.Scripts;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Combat.Projectiles
{
    public abstract class Throw : MonoBehaviour,IProjectile
    {
        [field:SerializeField]public PoolItemSO PoolItem { get; set; }
        [SerializeField] protected Rigidbody _rigidbody;
        [SerializeField] protected Collider _collider;

        public GameObject GameObject => gameObject;
        protected Pool _myPool;
        protected Entity _owner;
        public void InitProjectile(Entity owner, IProjectileShooter projectileShooter, Vector3 initPos, Vector3 direction, LayerMask excludeLayer)
        {
            transform.position = initPos;
            _collider.excludeLayers = excludeLayer;
            _rigidbody.AddForce(direction, ForceMode.Impulse);
            _rigidbody.AddTorque(direction);
            _owner = owner;
        }
        public virtual void ResetItem()
        {
            _rigidbody.linearVelocity = Vector3.zero;
        }

        public virtual void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }


    }
}
