using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace SHS.Scripts.Effects
{
    public class Footprint : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        [SerializeField] private float lifeTime = 10f;

        private Pool _pool;
        private float _returnTime;

        public GameObject GameObject => gameObject;

        private void Update()
        {
            if (Time.time >= _returnTime)
                _pool.Push(this);
        }

        public void Place(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _returnTime = Time.time + lifeTime;
        }

        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }

        public void ResetItem()
        {
        }
    }
}
