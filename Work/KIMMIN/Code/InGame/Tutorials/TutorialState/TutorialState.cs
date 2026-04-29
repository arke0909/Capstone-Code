using System;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class TutorialState : MonoBehaviour
    {
        [SerializeField] protected TutorialStateSO tutorialStateSO;
        
        protected TutorialController _tutorialController;
        protected Player _player;
        public event Action OnTutorialComplete;

        public virtual void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            _tutorialController = tutorialController;
            _player = player;
        }

        public virtual void EnterTutorial()
        {
            _tutorialController.SetDialogue(GetDialogue());
        }

        public virtual void ExitTutorial() { }
    
        protected void TutorialComplete()
        {
            OnTutorialComplete?.Invoke();
        }

        protected virtual string GetDialogue() => tutorialStateSO.dialogue;
    }
}