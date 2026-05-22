using Chipmunk.GameEvents;
using System;
using UnityEngine;

namespace Code.UI.Minimap.Minimaps
{
    public class ViewOnlyMinimap : MinimapBase
    {
        private void Update()
        {
            UpdatePlayerDot();
        }
    }
}