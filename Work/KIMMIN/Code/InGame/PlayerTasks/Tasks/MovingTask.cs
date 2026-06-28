using UnityEngine;
using Work.Code.Tutorials;

namespace Work.Code.PlayerTasks.Tasks
{
    public class MovingTask : PlayerTask
    {
        [SerializeField] private string taskDialogue;
        [SerializeField] private TutorialMarking marking;
        [SerializeField] private GameObject[] arrowObjects;

        public override void StartTask()
        {
            marking.OnDetectTarget += HandleDetectTarget;
            marking.SetEnable(true);
        }

        private void HandleDetectTarget()
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            marking.OnDetectTarget -= HandleDetectTarget;

            foreach (var arrow in arrowObjects)
            {
                arrow.SetActive(false);
            }
        }

        protected override string GetTaskText()
        {
            return taskDialogue;
        }
    }
}