using Code.UI.Minimap.Core;
using Code.UI.Minimap.SectionName;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;

namespace Code.UI.Minimap.Factory
{
    public class SectionNameFactory : MinimapFactory
    {
        [SerializeField] private PoolItemSO _minimapSectionNameItem;
        
        public override MinimapElement CreateUIElement(MinimapElementData data)
        {
            SectionNameText sectionNameText = _poolManager.Pop<SectionNameText>(_minimapSectionNameItem);
            sectionNameText.NormalizedPos = data.NormalizedPos;

            if (data.Owner is SectionNameSpot spot)
            {
                sectionNameText.NameText.SetText(spot.Name);;
                sectionNameText.NameText.fontSize = (int)spot.Size;
                sectionNameText.Area = spot.Area;
            }
            
            return sectionNameText;
        }
    }
}