using DewmoLib.ObjectPool.RunTime;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.Combat.Projectiles
{
    public abstract class Throw : MonoBehaviour, IPoolable
    {
        [field:SerializeField]public PoolItemSO PoolItem { get; set; }
        [SerializeField] protected Rigidbody _rigidbody;

        public GameObject GameObject => gameObject;
        protected Pool _myPool;
        protected Entity _owner;
        public virtual void InitThrow(Entity owner,Vector3 position,Vector3 dir)
        {
            transform.position = position;
            _rigidbody.AddForce(dir, ForceMode.Impulse);
            _rigidbody.AddTorque(dir);
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
