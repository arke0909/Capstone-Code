using System;
using System.Collections;
using System.Collections.Generic;
using Code.TimeSystem;
using Code.UI.Core;
using DewmoLib.Dependencies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.MapEvents
{
    public class MapEventUI : LayoutUIBase, IUIElement<MapEvent, float>
    {
        [SerializeField] private TextMeshProUGUI eventText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        
        private TimeController _timeController;
        private float _remainTime;
        private bool _isActive;

        [field: SerializeField] public RectTransform Rect { get; private set; }
        [field: SerializeField] public LayoutElement Layout { get; private set; }
        [field: SerializeField] public CanvasGroup Canvas { get; private set; }
        public event Action<MapEventUI> OnInActive;

        protected override void Awake()
        {
            base.Awake();
            DisableUI(true);
        }

        public void EnableFor(MapEvent evt, float remainTime = 0)
        {
            eventText.text = evt.EventName;
            icon.sprite = evt.MapEventSO.eventIcon;
            background.color = evt.MapEventSO.eventColor;
            _remainTime = remainTime / _timeController.TimeScale + 1f;
            
            _isActive = true;
            EnableUI();
        }

        private void Update()
        {
            if (_isActive)
                SetTimeText();
        }

        private void SetTimeText()
        {
            _remainTime -= Time.deltaTime;
            timeText.text = $"{_remainTime / 60:00} : {_remainTime % 60:00}";

            if (_remainTime <= 0)
            {
                DisableUI(true);
            }
        }

        public void SetTimeController(TimeController timeController)
        {
            _timeController = timeController;
        }

        public void Clear()
        {
            _isActive = false;
            _remainTime = 0;
            OnInActive?.Invoke(this);
        }
    }
}