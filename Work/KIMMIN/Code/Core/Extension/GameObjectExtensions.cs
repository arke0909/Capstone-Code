using UnityEngine;

namespace Work.Code.Core.Extension
{
    public static class GameObjectExtensions
    {
        public static bool IsInLayer(this GameObject go, LayerMask mask)
        {
            return (mask.value & (1 << go.layer)) != 0;
        }
    }
}