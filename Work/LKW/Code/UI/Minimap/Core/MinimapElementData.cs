using System;
using UnityEngine;

namespace Code.UI.Minimap.Core
{
    public enum ElementType
    {
        Marker,
        Enemy,
        SupplyIcon,
        SectionName,
        ShowItem
    }
    
    public class MinimapElementData
    {
        public ElementType Type { get; private set; }
        public string Id { get; } = Guid.NewGuid().ToString();
        public Vector2 NormalizedPos { get; set; }
        public Sprite IconSprite { get; }
        public bool SyncChildScale { get; }
        public object Owner { get; set; }

        public MinimapElementData(object owner, ElementType type,Sprite iconSprite
            , bool syncChildScale, bool initPosWithWorldPos = false)
        {
            Owner = owner;
            Type = type;
            IconSprite = iconSprite;
            SyncChildScale = syncChildScale;
        }
    }
}