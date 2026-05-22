using System;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.UI.Popup;
using DewmoLib.Dependencies;
using DG.Tweening;
using EasyTransition;
using Scripts.Players;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Work.Code.Core;
using Work.Code.GameEvents;
using Work.Code.UI.Core.Interaction;

namespace InGame.PlayerUI
{
    public class PlayerDeadUI : InteractableUI, IPopupProvider
    {
        [SerializeField] private TransitionSettings transition;
        [SerializeField] private Image fadeImage;
        [SerializeField] private PlayerInputSO playerInput;

        [Inject] private Player _player;
        private HealthCompo _healthCompo;
        private ChoiceCallback _choiceCallback = new();
        
        public event Action<Func<object>, ICallbackData> OnShowPopup;
        
        private void Start()
        {
            _healthCompo = _player.GetCompo<HealthCompo>();
            _healthCompo.OnValueChanged += HandleHealthChanged;

            _choiceCallback.OnAccept += HandleRespawn;
            _choiceCallback.OnReject += HandleToTitle;
            BindPopup(this);
        }

        private void HandleHealthChanged(StatSO vitalstat, float before, float after)
        {
            if (after <= 0 && !_player.IsDead)
            {
                _player.LocalEventBus.Raise(new PlayerDeadEvent());
                OnPlayerDead();
            }
        }

        private void OnPlayerDead()
        {
            playerInput.SetActive(false);
            fadeImage.DOFade(1f, 1f).OnComplete(AfterFaded);
        }

        private void AfterFaded()
        {
            Cursor.lockState = CursorLockMode.None;
            OnShowPopup?.Invoke(() => _player, _choiceCallback);
        }
        
        private void HandleRespawn()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            TransitionManager.Instance().Transition(currentScene, transition, 0f);
        }

        private void HandleToTitle()
        {
            TransitionManager.Instance().Transition(SceneDefine.TITLE_SCENE, transition, 0f);
        }
    }
}