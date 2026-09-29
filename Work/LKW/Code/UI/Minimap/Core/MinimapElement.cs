using Chipmunk.GameEvents;
using Code.Events;
using Code.UI.Minimap.Components;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Minimap.Core
{
    public abstract class MinimapElement : MonoBehaviour, IPoolable
    {
        
        [field:SerializeField] public bool SyncChildScale { get; set; }
        [field:SerializeField] public Vector2 NormalizedPos { get; set; }
        
        public string ID { get; set; }
        public RectTransform Rect { get; private set; }
        public Vector2 OriginSize { get; private set; }
        public Vector3 OriginScale { get; private set; }

        private MinimapIconHighlight _iconHighlight;
    
        protected virtual void Awake()
        {
            Rect = GetComponent<RectTransform>();
            OriginSize = Rect.sizeDelta;
            OriginScale = Rect.localScale;
        }

        public virtual void Initialize(MinimapElementData data)
        {
            ID = data.Id;
            NormalizedPos = data.NormalizedPos;
            SyncChildScale = data.SyncChildScale;

            SetHighlightActive(data.IsHighlighted);
        }

        public void SetHighlightActive(bool isActive)
        {
            if (isActive)
            {
                EnsureIconHighlight();
                _iconHighlight?.Play();
            }
            else
                _iconHighlight?.Stop();
        }
    
        public void RemoveSelf()
        {
            var evt = new RemoveMinimapElementEvent(ID);
            Bus.Raise(evt);
        }
        
        public void ReturnPool()
        {
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
            _iconHighlight?.Stop();
        }

        private void EnsureIconHighlight()
        {
            if (_iconHighlight != null) return;
            if (!TryGetComponent<Image>(out _)) return;

            _iconHighlight = GetComponent<MinimapIconHighlight>();
            if (_iconHighlight == null)
                _iconHighlight = gameObject.AddComponent<MinimapIconHighlight>();
        }
    
        #endregion
    }
}
