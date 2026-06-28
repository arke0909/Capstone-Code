using System;
using Chipmunk.GameEvents;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.GameEvents;

namespace InGame.PlayerUI
{
    public class PlayerGageUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI gageText;
        [SerializeField] private Image fill;

        private bool _isActive;
        private float _duration;
        private float _startTime;
        private string _gageText;
        private Action _onComplete;

        protected override void Awake()
        {
            base.Awake();
            EventBus.Subscribe<PlayerGageEvent>(HandlePlayerGage);
            EventBus.Subscribe<StopPlayerGageEvent>(HandleStopPlayerGage);
            DisableUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus.Unsubscribe<PlayerGageEvent>(HandlePlayerGage);
            EventBus.Unsubscribe<StopPlayerGageEvent>(HandleStopPlayerGage);
        }
        
        private void HandleStopPlayerGage(StopPlayerGageEvent evt)
        {
            ClearUI();
        }

        private void HandlePlayerGage(PlayerGageEvent evt)
        {
            EnableUI(true);

            _gageText = evt.GageText;
            _gageText = evt.GageText;
            _onComplete = evt.OnComplete;
            _duration = evt.Duration;
            
            _isActive = true;
            _startTime = Time.time;
            fill.rectTransform.localScale = new Vector3(0, 1, 1);
        }

        private void Update()
        {
            if (!_isActive)
                return;

            SetGageUI();
            CheckTimer();
        }

        private void CheckTimer()
        {
            if (Time.time - _startTime >= _duration)
            {
                var callback = _onComplete;
                ClearUI();
                callback?.Invoke();
            }
        }

        private void SetGageUI()
        {
            float time = Mathf.Min(Time.time - _startTime, _duration);
            float remain = _duration - time;
            fill.rectTransform.localScale = new Vector3(time / _duration, 1, 1);
            gageText.text = $"{_gageText} {remain:0.0}초";
        }

        public void ClearUI()
        {
            gageText.text = string.Empty;
            _onComplete = null;
            _isActive = false;
            DisableUI(true);
        }
    }
}