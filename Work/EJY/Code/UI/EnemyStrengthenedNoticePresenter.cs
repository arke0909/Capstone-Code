using Ami.BroAudio;
using Chipmunk.GameEvents;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Work.Code.GameEvents;

namespace Code.UI
{
    public class EnemyStrengthenedNoticePresenter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup noticeCanvasGroup;
        [SerializeField] private SoundID showSoundID;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;
        [SerializeField, Min(0f)] private float visibleDuration = 1f;
        [FormerlySerializedAs("fadeDuration")]
        [SerializeField, Min(0f)] private float fadeOutDuration = 2f;

        private RectTransform _noticeTransform;
        private TMP_Text _messageText;
        private string _defaultMessage;
        private Tween _fadeTween;

        private void Start()
        {
            if (TryInitializeNotice() == false)
            {
                enabled = false;
                return;
            }

            EventBus.Subscribe<DayChangeEvent>(HandleDayChange);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DayChangeEvent>(HandleDayChange);
            _fadeTween?.Kill();
        }

        private void HandleDayChange(DayChangeEvent evt)
        {
            ShowNotice(_defaultMessage);
        }

        [ContextMenu("Test Show Enemy Strengthened Notice")]
        private void TestShowNotice()
        {
            if (Application.isPlaying == false)
            {
                Debug.LogWarning($"[{nameof(EnemyStrengthenedNoticePresenter)}] ContextMenu test only works in Play Mode.", this);
                return;
            }

            ShowNotice(_defaultMessage);
        }

        private void ShowNotice(string message)
        {
            if (_noticeTransform == null && TryInitializeNotice() == false)
            {
                return;
            }

            _fadeTween?.Kill();
            _noticeTransform.SetAsLastSibling();

            if (_messageText != null)
            {
                _messageText.SetText(message);
            }

            noticeCanvasGroup.interactable = false;
            noticeCanvasGroup.blocksRaycasts = false;
            noticeCanvasGroup.alpha = fadeInDuration > 0f ? 0f : 1f;

            if (showSoundID.IsValid())
            {
                BroAudio.Play(showSoundID);
            }

            Sequence sequence = DOTween.Sequence().SetUpdate(true);

            if (fadeInDuration > 0f)
            {
                sequence.Append(noticeCanvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic));
            }

            if (visibleDuration > 0f)
            {
                sequence.AppendInterval(visibleDuration);
            }

            if (fadeOutDuration > 0f)
            {
                sequence.Append(noticeCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.OutCubic));
            }
            else
            {
                sequence.AppendCallback(() => noticeCanvasGroup.alpha = 0f);
            }

            _fadeTween = sequence.OnComplete(() =>
            {
                if (noticeCanvasGroup != null)
                {
                    noticeCanvasGroup.alpha = 0f;
                }
            });
        }

        private bool TryInitializeNotice()
        {
            if (noticeCanvasGroup == null)
            {
                Debug.LogError($"[{nameof(EnemyStrengthenedNoticePresenter)}] Notice CanvasGroup is not assigned.", this);
                return false;
            }

            _noticeTransform = noticeCanvasGroup.transform as RectTransform;
            if (_noticeTransform == null)
            {
                Debug.LogError($"[{nameof(EnemyStrengthenedNoticePresenter)}] Notice object must use RectTransform.", noticeCanvasGroup);
                return false;
            }

            _messageText = noticeCanvasGroup.GetComponentInChildren<TMP_Text>(true);
            if (_messageText == null)
            {
                Debug.LogError($"[{nameof(EnemyStrengthenedNoticePresenter)}] Message text is not assigned.", noticeCanvasGroup);
                return false;
            }

            _defaultMessage = _messageText.text;

            noticeCanvasGroup.alpha = 0f;
            noticeCanvasGroup.interactable = false;
            noticeCanvasGroup.blocksRaycasts = false;
            return true;
        }
    }
}
