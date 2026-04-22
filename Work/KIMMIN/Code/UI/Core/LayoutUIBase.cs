using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Core
{
    [RequireComponent(typeof(LayoutElement))]
    public class LayoutUIBase : UIBase
    {
        private LayoutElement _layoutElement;

        protected override void Awake()
        {
            base.Awake();
            _layoutElement = GetComponent<LayoutElement>();
        }

        public override void EnableUI(bool hasTween = false)
        {
            base.EnableUI(hasTween);
            _layoutElement.ignoreLayout = false;
        }

        public override void DisableUI(bool hasTween = false)
        {
            base.DisableUI(hasTween);
            _layoutElement.ignoreLayout = true;
        }
    }
}