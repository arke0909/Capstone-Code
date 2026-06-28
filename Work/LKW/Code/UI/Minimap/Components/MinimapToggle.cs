using Code.UI.Minimap;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;

namespace Code.UI.Minimap.Components
{
    public class MinimapToggle : MonoBehaviour
    {
        [Inject] private Player _player;
        [SerializeField] private MinimapUI minimapUI;

        private void OnEnable()
        {
            if (_player != null)
                _player.PlayerInput.OnMinimapPressed += HandlePressed;
        }

        private void OnDisable()
        {
            if (_player != null)
                _player.PlayerInput.OnMinimapPressed -= HandlePressed;
        }

        private void HandlePressed()
        {
            minimapUI.ToggleUI(true);
        }
    }
}