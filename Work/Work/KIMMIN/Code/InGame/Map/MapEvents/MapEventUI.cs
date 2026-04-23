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
    public class MapEventUI : MonoBehaviour, IUIElement<MapEvent, float>
    {
        [SerializeField] private TextMeshProUGUI eventText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        
        private TimeController _timeController;
        private WaitForSeconds _disableDelay = new(5f);
        private float _remainTime;
        private bool _isActive;
        private bool _hasCooltime;

        [field: SerializeField] public RectTransform Rect { get; private set; }
        [field: SerializeField] public LayoutElement Layout { get; private set; }
        [field: SerializeField] public CanvasGroup Canvas { get; private set; }
        public event Action<MapEventUI> OnInActive;
        
        public void EnableFor(MapEvent evt, float remainTime = 0)
        {
            gameObject.SetActive(true);

            eventText.text = evt.EventName;
            icon.sprite = evt.MapEventSO.eventIcon;
            background.color = evt.MapEventSO.eventColor;
            _remainTime = remainTime / _timeController.TimeScale;
            
            _isActive = true;
            _hasCooltime = remainTime != 0;
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
                timeText.text = string.Empty;
                if (_hasCooltime == false)
                    UIUtility.FadeUI(gameObject, 0.1f, true, Clear);
                else
                    StartCoroutine(DisableUIRoutine());
            }
        }

        private IEnumerator DisableUIRoutine()
        {
            yield return _disableDelay;
            UIUtility.FadeUI(gameObject, 0.1f, true, Clear);
        }

        public void SetTimeController(TimeController timeController)
        {
            _timeController = timeController;
        }

        public void Clear()
        {
            gameObject.SetActive(false);
            _isActive = false;
            _remainTime = 0;
            OnInActive?.Invoke(this);
        }
    }
}