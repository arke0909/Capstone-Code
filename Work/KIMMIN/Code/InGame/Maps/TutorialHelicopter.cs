using Scripts.Entities;
using UnityEngine;
using UnityEngine.Playables;
using Code.ItemContainers;
using Scripts.GameSystem;
using UnityEngine.InputSystem;
using Work.Code.Tutorials;

namespace Work.Code.Map
{
    public class TutorialHelicopter : InteractableStructure, IInteractable
    {
        [SerializeField] private PlayableDirector helicopterCutscene;
        [SerializeField] private SceneTransitionSignal sceneTransitionSignal;
        [SerializeField] private SkipCutsceneUI skipCutsceneUI;
        [SerializeField] private float skipHoldDuration = 1f;

        private float _skipHoldTime;
        private bool _isSkipped;
        private bool _isSkipHolding;

        private void Update()
        {
            if (helicopterCutscene == null || helicopterCutscene.state != PlayState.Playing || _isSkipped)
            {
                ResetSkipHold();
                return;
            }

            if (!Keyboard.current.spaceKey.isPressed)
            {
                ResetSkipHold();
                return;
            }

            if (!_isSkipHolding)
            {
                _isSkipHolding = true;
                skipCutsceneUI?.StartProgress(skipHoldDuration);
            }

            _skipHoldTime += Time.deltaTime;

            if (_skipHoldTime >= skipHoldDuration)
                SkipCutscene();
        }

        public override void Interact(Entity interactor)
        {
            _skipHoldTime = 0f;
            _isSkipped = false;
            _isSkipHolding = false;
            skipCutsceneUI?.EnableUI();
            skipCutsceneUI?.SetProgressImmediately(0f);
            helicopterCutscene?.Play();
        }

        private void SkipCutscene()
        {
            _isSkipped = true;
            _isSkipHolding = false;
            skipCutsceneUI?.SetProgressImmediately(1f);
            helicopterCutscene.time = helicopterCutscene.duration;
            helicopterCutscene.Evaluate();
            helicopterCutscene.Stop();
            sceneTransitionSignal?.OnReceive();
        }

        private void ResetSkipHold()
        {
            _skipHoldTime = 0f;

            if (!_isSkipHolding)
                return;

            _isSkipHolding = false;
            skipCutsceneUI?.ResetProgress();
        }
    }
}
