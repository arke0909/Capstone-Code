using Scripts.Players;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Popup
{
    public class GameOverPopup : BasePopup<Player, ChoiceCallback>
    {
        [SerializeField] private Button respawnButton;
        [SerializeField] private Button toTitleButton;
        
        private Player _player;

        protected override void Awake()
        {
            base.Awake();
            respawnButton.onClick.AddListener(HandleRespawnClicked);
            toTitleButton.onClick.AddListener(HandleToTitleClicked);
        }

        private void HandleToTitleClicked()
        {
            _callback.OnReject?.Invoke();
        }

        private void HandleRespawnClicked()
        {
            _callback.OnAccept?.Invoke();
        }

        protected override void ShowPopup(Player player, ChoiceCallback callback)
        {
            _callback = callback;
        }
    }
}