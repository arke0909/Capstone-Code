using Chipmunk.GameEvents;
using Code.Events;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.UI.Minimap.Core
{
    public abstract class MinimapElement : MonoBehaviour, IPoolable
    {
        [field:SerializeField] public bool SyncChildScale { get; set; }
        public string ID { get; set; }
        public RectTransform Rect { get; private set; }
        [field:SerializeField] public Vector2 NormalizedPos { get; set; }
        public Vector2 OriginSize { get; set; }

        protected virtual void Awake()
        {
            Rect = GetComponent<RectTransform>();
            OriginSize = Rect.sizeDelta;
        }

        public void RemoveSelf()
        {
            var evt = new RemoveMinimapElementEvent(ID);
            Bus.Raise(evt);
            _myPool.Push(this);
        }

        #region Pool

        private Pool _myPool;
        [field:SerializeField] public PoolItemSO PoolItem { get; set; }
        public GameObject GameObject => gameObject;
        
        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }

        public virtual void ResetItem()
        {
        }

        #endregion

       
    }
}