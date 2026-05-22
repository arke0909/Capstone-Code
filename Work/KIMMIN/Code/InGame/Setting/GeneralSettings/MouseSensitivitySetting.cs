using System;
using DewmoLib.Dependencies;
using SHS.Scripts.Crosshairs;
using UnityEngine;
using Work.Code.UI.ScrollBar;

namespace Work.Code.Setting.GeneralSetting
{
    public class MouseSensitivitySetting : MonoBehaviour
    {
        [SerializeField] private BaseScrollbar sensitivityScrollbar;

        [Inject] private CrosshairBehavior _crosshairBehavior;

        private void Awake()
        {
            sensitivityScrollbar.OnValueChanged += HandleChangeSensitivity;
        }

        private void OnDestroy()
        {
            sensitivityScrollbar.OnValueChanged -= HandleChangeSensitivity;
        }

        private void HandleChangeSensitivity(float value)
        {
            _crosshairBehavior.SetSensitivity(value);
        }
    }
}