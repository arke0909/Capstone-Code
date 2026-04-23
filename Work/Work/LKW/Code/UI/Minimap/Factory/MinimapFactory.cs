using Code.UI.Minimap.Core;
using DewmoLib.Dependencies;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.UI.Minimap.Factory
{
    public abstract class MinimapFactory : MonoBehaviour
    {
        [Inject] protected PoolManagerMono _poolManager;

        [field:SerializeField] public ElementType Type { get; set; }
        public abstract MinimapElement CreateUIElement(MinimapElementData data);
    }
}