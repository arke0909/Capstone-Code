using UnityEngine;

namespace Work.Code.Tutorials
{
    public class MovingTutorialState : TutorialState
    { 
        [SerializeField] private TutorialMarking marking;

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            
            marking.OnDetectTarget += HandleDetectTarget;
            marking.SetEnable(true);
        }

        private void HandleDetectTarget()
        {
            TutorialComplete();
        }

        public override void ExitTutorial()
        {
            marking.OnDetectTarget -= ExitTutorial;
        }
    }
}