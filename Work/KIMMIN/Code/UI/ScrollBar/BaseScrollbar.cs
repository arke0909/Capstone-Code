using System;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.UI.ScrollBar
{
    [RequireComponent(typeof(Scrollbar))]
    public class BaseScrollbar : UIBase
    {
        [SerializeField] private TextMeshProUGUI valueText;
        
        [field: SerializeField] public float MaxValue { get; set; } = 1f;
        [field: SerializeField] public float MinValue { get; set; } = 0f;
        [field: SerializeField] public float DefaultValue { get; set; } = 0.5f;
        
        public Scrollbar Scrollbar { get; private set; }
        
        public event Action<float> OnValueChanged;

        protected override void Awake()
        {
            Scrollbar = GetComponent<Scrollbar>();
            Scrollbar.onValueChanged.AddListener(HandleChangeValue);
            Scrollbar.value = Mathf.InverseLerp(MinValue, MaxValue, DefaultValue);
        }

        private void HandleChangeValue(float value)
        {
            float adjustedValue = Mathf.Lerp(MinValue, MaxValue, value);
            
            if(valueText != null)
                valueText.text = adjustedValue.ToString("0.0");
            
            OnValueChanged?.Invoke(adjustedValue);
        }

        protected override void OnDestroy()
        {
            Scrollbar.onValueChanged.RemoveAllListeners();
        }
    }
}