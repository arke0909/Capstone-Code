using Chipmunk.GameEvents;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Work.Code.GameEvents;

namespace Work.Code.UI.Misc
{
    public class SystemMessagePresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageTemplate;
        [SerializeField, Min(0f)] private float fadeInDuration = 1f;
        [SerializeField, Min(0f)] private float visibleDuration = 5f;
        [SerializeField, Min(0f)] private float fadeOutDuration = 1f;

        private void Awake()
        {
            if (messageTemplate == null)
            {
                Debug.LogError($"[{nameof(SystemMessagePresenter)}] Message template is not assigned.", this);
                enabled = false;
                return;
            }

            messageTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<SystemMessageEvent>(HandleSystemMessage);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<SystemMessageEvent>(HandleSystemMessage);
        }

        private void HandleSystemMessage(SystemMessageEvent evt)
        {
            ShowMessage(evt.Message);
        }

        [UnityEngine.ContextMenu("Test Show System Message")]
        private void TestShowMessage()
        {
            if (Application.isPlaying == false)
            {
                Debug.LogWarning($"[{nameof(SystemMessagePresenter)}] ContextMenu test only works in Play Mode.", this);
                return;
            }

            ShowMessage("시스템 메시지 테스트");
        }

        private void ShowMessage(string message)
        {
            if (messageTemplate == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            TextMeshProUGUI messageText = Instantiate(messageTemplate, messageTemplate.transform.parent);
            messageText.name = $"System Message - {message}";
            messageText.SetText(message);
            messageText.raycastTarget = false;
            messageText.alpha = fadeInDuration > 0f ? 0f : 1f;
            messageText.gameObject.SetActive(true);
            messageText.transform.SetAsLastSibling();

            Sequence sequence = DOTween.Sequence().SetUpdate(true);

            if (fadeInDuration > 0f)
            {
                sequence.Append(messageText.DOFade(1f, fadeInDuration).SetEase(Ease.OutCubic));
            }

            if (visibleDuration > 0f)
            {
                sequence.AppendInterval(visibleDuration);
            }

            if (fadeOutDuration > 0f)
            {
                sequence.Append(messageText.DOFade(0f, fadeOutDuration).SetEase(Ease.OutCubic));
            }

            sequence
                .SetLink(messageText.gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(() => Destroy(messageText.gameObject));
        }
    }
}
