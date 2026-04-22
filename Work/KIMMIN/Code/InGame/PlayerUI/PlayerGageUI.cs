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
            DisableUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus.Unsubscribe<PlayerGageEvent>(HandlePlayerGage);
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
            if (!_isActive) return;
            
            float time = Time.time - _startTime;
            string remainTime = (_duration - time).ToString("0.0");
            fill.rectTransform.localScale = new Vector3(time / _duration, 1, 1);
            gageText.text = $"{_gageText} {remainTime}초";

            if (Time.time - _startTime >= _duration)
            {
                _onComplete?.Invoke();
                Clear();
            }
        }

        public void Clear()
        {
            gageText.text = string.Empty;
            _onComplete = null;
            _isActive = false;
            DisableUI(true);
        }
    }
}