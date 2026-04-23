using System;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

namespace Work.Code.Misc
{
    public class BulletHole : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifeTime = 10f;
        [SerializeField] private DecalProjector decal;
        [SerializeField] private Material[] bullets;
        
        private bool _isActive = false;
        private float _time;
        private Pool _pool;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public GameObject GameObject => gameObject;

        private void Update()
        {
            if (_isActive && Time.time - _time > lifeTime)
            {
                _isActive = false;
                _pool.Push(this);
            }
        }

        public void InitHole(Vector3 position, Vector3 normal)
        {
            transform.position = position - normal * 0.001f;
            transform.rotation = Quaternion.LookRotation(-normal);
                decal.material = bullets[Random.Range(0, bullets.Length)];
            _time = Time.time;
            _isActive = true;
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