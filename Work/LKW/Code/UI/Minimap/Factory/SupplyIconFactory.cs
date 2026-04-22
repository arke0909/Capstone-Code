using Code.UI.Minimap.Core;
using Code.UI.Minimap.Markers;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Minimap.Factory
{
    public class SupplyIconFactory : MinimapFactory
    {
        [SerializeField] private PoolItemSO markerItem;
        [SerializeField] private Sprite supplySprite;
        
        public override MinimapElement CreateUIElement(MinimapElementData data)
        {
            SupplyIcon supplyIcon = _poolManager.Pop<SupplyIcon>(markerItem);
            
            supplyIcon.GetComponent<Image>().sprite = supplySprite;
            
            supplyIcon.NormalizedPos = data.NormalizedPos;

            supplyIcon.ID = data.Id;
            
            supplyIcon.SetLifeTimer();

            return supplyIcon;
        }
    }
}