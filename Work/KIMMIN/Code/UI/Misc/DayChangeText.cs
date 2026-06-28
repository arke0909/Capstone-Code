using System;
using Chipmunk.GameEvents;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Work.Code.GameEvents;

namespace Work.Code.UI.Misc
{
    public class DayChangeText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI dayText;
        
        private int _currentDay = 0;
        private Sequence _daySequence;
        
        private void Awake()
        {
            EventBus.Subscribe<DayChangeEvent>(HandleDayChange);
            
            _daySequence = DOTween.Sequence()
                .SetAutoKill(false)
                .Pause();
            
            _daySequence.Append(dayText.DOFade(1f, 1f));
            _daySequence.AppendInterval(5f);
            _daySequence.Append(dayText.DOFade(0f, 1f));
            ChangeDay();
        }

        private void HandleDayChange(DayChangeEvent evt)
        {
            ChangeDay();
        }

        private void ChangeDay()
        {
            _currentDay++;
            dayText.text = $"{_currentDay}일차";
            ShowDayChangeEffect();
        }

        public void ShowDayChangeEffect()
        {
            dayText.alpha = 0f;
            _daySequence?.Restart();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayChangeEvent>(HandleDayChange);
        }
    }
}