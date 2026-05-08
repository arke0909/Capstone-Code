using System;
using UnityEngine;

namespace Code.UI.Minimap.Minimaps
{
    public class ViewOnlyMinimap : MinimapBase
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            EnableUI();
        }

        private void Update()
        {
            UpdatePlayerDot();
        }
    }
}