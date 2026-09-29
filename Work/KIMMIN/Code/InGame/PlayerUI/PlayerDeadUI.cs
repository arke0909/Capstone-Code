using System;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Modules.StatSystem;
using Code.TimeSystem;
using Code.UI.Core;
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
    public class PlayerDeadUI : InteractableUI
    {
        [SerializeField] private TransitionSettings transition;
        [SerializeField] private Image fadeImage;
        [SerializeField] private PlayerInputSO playerInput;

        [Inject] private Player _player;
        private HealthCompo _healthCompo;
        private ChoiceCallback _choiceCallback = new();
        
        private void Start()
        {
            _healthCompo = _player.GetCompo<HealthCompo>();
            _healthCompo.OnValueChanged += HandleHealthChanged;

            _choiceCallback.OnAccept += HandleRespawn;
            _choiceCallback.OnReject += HandleToTitle;
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
            UIManager.Instance.SetLockState(true);
        }

        private void AfterFaded()
        {
            Cursor.lockState = CursorLockMode.None;
            OverlayUIManager.Instance.ShowPopup(_player, _choiceCallback);
            TimeController.Instance.SetPause(true);
            Time.timeScale = 0f;
        }
        
        private void HandleRespawn()
        {
            TimeController.Instance.SetPause(false);
            Time.timeScale = 1f;
            string currentScene = SceneManager.GetActiveScene().name;
            TransitionManager.Instance().Transition(currentScene, transition, 0f);
        }

        private void HandleToTitle()
        {
            TimeController.Instance.SetPause(false);
            Time.timeScale = 1f;
            TransitionManager.Instance().Transition(SceneDefine.TITLE_SCENE, transition, 0f);
        }
    }
}
