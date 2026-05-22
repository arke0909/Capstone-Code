using UnityEngine;

namespace Work.Code.Tutorials
{
    public class MovingTutorialState : TutorialState
    { 
        [SerializeField] private TutorialMarking marking;
        [SerializeField] private GameObject[] arrowObjs;

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

            foreach (var arrow in arrowObjs)
            {
                arrow.gameObject.SetActive(false);
            }
        }
    }
}