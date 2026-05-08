using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Code.UI.Minimap;
using Code.UI.Minimap.Core;
using Code.UI.Minimap.Factory;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;

namespace Code.UI.Minimap.Minimaps
{
    public class FixedMinimapUI : MinimapBase
    {
        private void Start()
        {
            ShowUIOnInspector();
        }

        private void Update()
        {
            UpdatePlayerDot();
            SetMapImageOffset();
        }

        private void HandleMinimapPressed() => ToggleUI(true);
        
        private void SetMapImageOffset()
        {
            Vector2 followPos = -playerDot.anchoredPosition;
            miniMapRect.anchoredPosition = followPos;
        }
    }
}