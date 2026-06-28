using System;
using Code.UI.Core;
using UnityEngine;

namespace Work.Code.PlayerUI.Menu
{
    public class AlertController : MonoBehaviour
    {
        [field: SerializeField] public UIBase AlertUI { get; private set; }

        protected virtual void Start()
        {
            AlertUI.DisableUI();
        }

        public void SetAlert(bool isVisible)
        {
            if(isVisible)
                AlertUI.EnableUI();
            else 
                AlertUI.DisableUI();
        }
    }
}