using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Popup
{
    public class AlertPopup : BasePopup<string, ConfirmCallback>
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button confirmButton;

        protected override void Awake()
        {
            confirmButton.onClick.AddListener(HandleConfirm);
        }

        protected override void OnDestroy()
        {
            confirmButton.onClick.RemoveListener(HandleConfirm);
        }

        private void HandleConfirm()
        {
            _callback.OnConfirm?.Invoke();
        }

        protected override void ShowPopup(string data, ConfirmCallback callback)
        {
            messageText.text = data;
        }
    }
}