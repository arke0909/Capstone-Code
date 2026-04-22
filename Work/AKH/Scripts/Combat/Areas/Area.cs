using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Scripts.Combat.Areas
{
    public abstract class Area : MonoBehaviour,IPoolable
    {
        [SerializeField] protected float damageTick = 0.5f;
        [SerializeField] protected float floorDuration = 5f;
        [SerializeField] protected PoolItemSO poolItem;
        [SerializeField] protected ParticleSystem particle;
        public PoolItemSO PoolItem => poolItem;
        public GameObject GameObject => gameObject;

        protected Entity _owner;
        protected Pool _myPool;
        protected int _remainingTicks;
        protected float _tickTimer = 0;
        protected bool _isWork;

        protected virtual void Awake()
        {
            var particleMain = particle.main;
            particleMain.duration = floorDuration + 1.5f;
        }

        public virtual void Init(Entity owner, Vector3 position)
        {
            _owner = owner;

            _remainingTicks = Mathf.FloorToInt(floorDuration / damageTick);

            _isWork = true;
            position.y += 0.5f;
            transform.position = position;

            particle.Play();
        }

        private void Update()
        {
            if (!_isWork) return;

            _tickTimer += Time.deltaTime;

            if (_tickTimer >= damageTick && _remainingTicks > 0)
            {
                _tickTimer -= damageTick;
                _remainingTicks--;

                TickElapsed();

                if (_remainingTicks == 0)
                {
                    particle.Stop();
                    _myPool.Push(this);
                }
            }
        }
        
        public void SetFloorDuration(float duration) => floorDuration = duration;

        protected abstract void TickElapsed();

        public virtual void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public virtual void ResetItem()
        {
            _tickTimer = 0;
        }
    }
}
