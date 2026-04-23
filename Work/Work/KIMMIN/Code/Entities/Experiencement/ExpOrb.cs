using System;
using System.Collections;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Work.Code.Entities.Experiencement
{
    public class ExpOrb : MonoBehaviour, IPoolable
    {
        [SerializeField] private TrailRenderer orbTrail;
        private Pool _pool;
        
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        public bool CanCollect { get; private set; }
        public Action OnCollected { get; private set; }
        public GameObject GameObject => gameObject;
        
        public void SetUpPool(Pool pool)
        {
            _pool = pool;
        }
        
        public void CollectingOrb()
        {
            OnCollected?.Invoke();
            OnCollected = null;
            _pool.Push(this);
        }

        public void SubscribeOnCollect(Action onCollected = null)
        {
            OnCollected = onCollected;
        }

        public void InitOrb(Vector3 position, float dropRange)
        {
            CanCollect = false;
            orbTrail?.Clear();
            Vector3 destination = GetDestination(position, dropRange);
            StartCoroutine(ParabolaMove(position, destination, 0.6f, 2f));
        }

        private Vector3 GetDestination(Vector3 position, float range)
        {
            transform.position = position;
            transform.transform.localScale = Vector3.one * Random.Range(0.06f, 0.1f);
            
            Vector3 targetPos = transform.position + Random.insideUnitSphere * range;
            targetPos.y = 4f;
            return targetPos;
        }
        
        private IEnumerator ParabolaMove(Vector3 start, Vector3 end, float duration, float height)
        {
            float time = 0f;
            bool triggered = false;
            
            while (time < duration)
            {
                float t = time / duration;
                
                if (!triggered && t >= 0.5f)
                {
                    CanCollect = true;
                    triggered = true;
                }
                
                Vector3 pos = Vector3.Lerp(start, end, t);
                pos.y += height * 4f * t * (1f - t);

                transform.position = pos;
                time += Time.deltaTime;
                yield return null;
            }

            transform.position = end;
        }

        public void ResetItem()
        {
            orbTrail?.Clear();
        }
    }
}