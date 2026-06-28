using System;
using Code.TimeSystem;
using Code.UI.Core;
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
        private float _endTime;
        private float _lastDisplayRemainTime;
        private bool _isActive;

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
            _endTime = GetCurrentTime() + remainTime;
            _remainTime = remainTime;
            _lastDisplayRemainTime = CalculateDisplayRemainTime(_remainTime);
            
            _isActive = true;
            EnableUI();
            SetTimeText();
        }

        private void Update()
        {
            if (_isActive)
                SetTimeText();
        }

        private void SetTimeText()
        {
            _remainTime = Mathf.Max(0f, _endTime - GetCurrentTime());
            float displayRemainTime = CalculateDisplayRemainTime(_remainTime);
            int seconds = Mathf.CeilToInt(displayRemainTime);

            timeText.text = $"{seconds / 60:00} : {seconds % 60:00}";

            if (_remainTime <= 0)
            {
                Clear();
            }
        }

        private float GetCurrentTime()
        {
            return _timeController != null ? _timeController.TotalTime : Time.time;
        }

        private float CalculateDisplayRemainTime(float remainTime)
        {
            if (_timeController == null)
                return remainTime;

            if (_timeController.TimeScale <= 0f)
                return _lastDisplayRemainTime > 0f ? _lastDisplayRemainTime : remainTime;

            _lastDisplayRemainTime = remainTime / _timeController.TimeScale;
            return _lastDisplayRemainTime;
        }

        public void SetTimeController(TimeController timeController)
        {
            _timeController = timeController;
        }

        public void Clear()
        {
            _isActive = false;
            _remainTime = 0;
            _endTime = 0;
            _lastDisplayRemainTime = 0;
            DisableUI(true);
            OnInActive?.Invoke(this);
        }
    }
}
