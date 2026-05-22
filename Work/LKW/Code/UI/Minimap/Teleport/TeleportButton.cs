using Code.UI.Minimap.Core;
using Scripts.GameSystem.Structures;
using UnityEngine;

namespace Minimap.Teleport
{
    public class TeleportButton : MinimapElement
    {
        public TeleportStructure TeleportStructure { get; set; }

        public void Teleport()
          =>  TeleportStructure?.Teleport();
    }
}