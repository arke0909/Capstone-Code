using System;
using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Effects
{
    public class PoolingEffect : MonoBehaviour, IPoolable
    {
        [field: SerializeField] public PoolItemSO PoolItem { get; private set; }
        [SerializeField] private bool backToPoolOnEnd = false;
        [ShowIf("backToPoolOnEnd")][SerializeField] private float autoReturnTime = 3f;
        public GameObject GameObject => gameObject;

        private Pool _myPool;
        private Vector3 _originScale;
        [SerializeField] private GameObject effectObject;
        private IPlayableVFX _playableVFX;

        private void Awake()
        {
            _originScale = transform.localScale;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
            _playableVFX = effectObject.GetComponent<IPlayableVFX>();
            Debug.Assert(_playableVFX != null, "Effect object must have IPlayableVFX compo");
        }

        public void ResetItem()
        {
            transform.localPosition = Vector3.zero;
            _playableVFX.StopVFX();
        }

        public void PlayVFX(Vector3 position, Quaternion rotation, float scale)
        {
            _originScale *= scale;
            PlayVFX(position, rotation);
        }
        
        public void PlayVFX(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
            _playableVFX.PlayVFX(position, rotation);
            if (backToPoolOnEnd)
                DOVirtual.DelayedCall(autoReturnTime, () => _myPool.Push(this));
        }
        
        public void ReturnToPool()
        {
            _myPool.Push(this);
        }

        private void OnValidate()
        {
            if (effectObject == null) return;
            _playableVFX = effectObject.GetComponent<IPlayableVFX>();
            if (_playableVFX == null)
            {
                effectObject = null;
                Debug.LogError("effectObject is null");
            }
        }
    }
}