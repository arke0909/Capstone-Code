using System;
using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using DewmoLib.Dependencies;
using Scripts.Entities.Vitals;
using Scripts.Players;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.PlayerUI
{
    public class PlayerSteminaUI : MonoBehaviour
    {
        [SerializeField] private StaminaUI staminaUI;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Vector3 offset = new Vector3(1f, 0.5f, 0.25f);

        [Inject] private Player _player;
        private LocalEventBus _eventBus;
        private Camera _cam;
        
        private void Start()
        {
            _cam = Camera.main;
            canvas.worldCamera = _cam;
            
            _eventBus = _player.LocalEventBus;
            _eventBus.Subscribe<StaminaChangeEvent>(HandleStaminaChange);
            staminaUI.DisableUI();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<StaminaChangeEvent>(HandleStaminaChange);
        }

        private void HandleStaminaChange(StaminaChangeEvent evt)
        {
            if (evt.CurrentStamina >= evt.MaxStamina)
            {
                if (staminaUI.IsActive)
                    staminaUI.DisableUI(true);
                
                return;
            }

            if (!staminaUI.IsActive)
            {
                staminaUI.EnableUI(true);
            }
            
            staminaUI.SetFill(evt.CurrentStamina / evt.MaxStamina);
        }

        private void LateUpdate()
        {
            if (_cam == null) return;
            gameObject.transform.forward = _cam.transform.forward;
            gameObject.transform.position = _player.transform.position + offset;
        }
    }
}