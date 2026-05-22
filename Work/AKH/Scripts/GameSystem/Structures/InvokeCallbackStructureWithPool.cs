using DewmoLib.ObjectPool.RunTime;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using static EPOOutline.TargetStateListener;

namespace Scripts.GameSystem.Structures
{
    public class InvokeCallbackStructureWithPool : InvokeCallbackStructure, IPoolable
    {
        [field: SerializeField] public PoolItemSO PoolItem { get; set; }
        private Pool _myPool;

        public void ResetItem()
        {
            _callback = null;
        }

        public void SetUpPool(Pool pool)
        {
            _myPool = pool;
        }
        protected override void OnDespawnComplete()
        {
            _myPool.Push(this);
        }
    }
}
