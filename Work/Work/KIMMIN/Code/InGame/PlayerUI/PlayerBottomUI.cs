using System;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using UnityEngine;

namespace InGame.PlayerUI
{
    public class PlayerBottomUI : MonoBehaviour
    {
        private void Awake()
        {
            EventBus.Subscribe<PlayerUIEvent>(HandleToggleInventory);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<PlayerUIEvent>(HandleToggleInventory);
        }

        private void HandleToggleInventory(PlayerUIEvent evt)
        {
            UIUtility.FadeUI(gameObject, 0.1f, evt.IsEnabled);
        }
    }
}