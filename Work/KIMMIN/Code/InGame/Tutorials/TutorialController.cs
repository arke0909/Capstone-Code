using System;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using Work.Code.UI.Misc;

namespace Work.Code.Tutorials
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private TypeEffectText dialogueText;

        [Inject] private Player _player;
        private TutorialState[] _tutorialStates;
        private TutorialState _currentState;
        private int _tutorialIndex;

        public Action OnTutorialComplete;

        private void Start()
        {
            _tutorialStates = GetComponentsInChildren<TutorialState>();
            Debug.Assert(_tutorialStates.Length != 0, "TutorialStates is empty");
            
            foreach (TutorialState state in _tutorialStates)
            {
                state.InitializeTutorial(this, _player);
            }
            
            ChangeTutorialState(_tutorialStates[_tutorialIndex]);
        }

        private void HandleTutorialComplete()
        {
            _tutorialIndex++;
            
            if (_tutorialIndex >= _tutorialStates.Length)
            {
                OnTutorialComplete?.Invoke();
                return;
            }
            
            TutorialState newState = _tutorialStates[_tutorialIndex];
            ChangeTutorialState(newState);
        }

        private void ChangeTutorialState(TutorialState newState)
        {
            if (_currentState != null)
            {
                _currentState.ExitTutorial();
                _currentState.OnTutorialComplete -= HandleTutorialComplete;
            }
            
            _currentState = newState;
            _currentState.OnTutorialComplete += HandleTutorialComplete;
            _currentState.EnterTutorial();
        }

        public void SetDialogue(string dialogue, bool nonEffect = false)
        {
            dialogueText.SetText(dialogue, nonEffect);
        }

        private void OnDestroy()
        {
            if (_currentState != null)
                _currentState.OnTutorialComplete -= HandleTutorialComplete;
        }
    }
}