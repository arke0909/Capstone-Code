using Code.UI.Minimap.Core;
using UnityEngine;
using UnityEngine.UI;
using  Code.UI.Minimap.Markers;
using DewmoLib.ObjectPool.RunTime;

namespace Code.UI.Minimap.Factory
{
    public class MarkerFactory : MinimapFactory
    {
        [SerializeField] private PoolItemSO markerItem;
        
        public override MinimapElement CreateUIElement(MinimapElementData data)
        {
            Marker marker = _poolManager.Pop<Marker>(markerItem);
            marker.GetComponent<Image>().sprite = data.IconSprite;
            marker.NormalizedPos = data.NormalizedPos;
            marker.ID = data.Id;

            return marker;
        }
    }
}