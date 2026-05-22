using Code.UI.Minimap.Core;
using Code.UI.Minimap.SectionName;
using DewmoLib.ObjectPool.RunTime;
using Minimap.Teleport;
using Scripts.GameSystem.Structures;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.UI.Minimap.Factory
{
    public class TeleportFactory : MinimapFactory
    { 
        [SerializeField] private PoolItemSO teleportButtonItem;
        
        public override MinimapElement CreateUIElement(MinimapElementData data)
        {
            TeleportButton teleportButton = _poolManager.Pop<TeleportButton>(teleportButtonItem);
            teleportButton.NormalizedPos = data.NormalizedPos;

            if (data.Owner is TeleportStructure structure)
            {
                teleportButton.TeleportStructure = structure;
            }
            
            return teleportButton;
        }
    }
}