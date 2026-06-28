using System.Linq;
using UnityEngine;
using Work.Code.Tutorials;

namespace Work.Code.PlayerTasks
{
    public class TaskGroup : MonoBehaviour
    {
        private PlayerTask[] _tasks;
        private TaskCompleteInteraction[] _completeInteractions;
        
        public PlayerTask[] Tasks
        {
            get
            {
                if (_tasks == null)
                    GetTasks();

                return _tasks;
            }
        }

        public TaskCompleteInteraction[] CompleteInteractions => _completeInteractions;
        
        private void Awake()
        {
            GetTasks();
        }

        private void GetTasks()
        {
            _tasks = GetComponentsInChildren<PlayerTask>(true)
                .OrderByDescending(task => task.Priority).ToArray();
            
            _completeInteractions = GetComponentsInChildren<TaskCompleteInteraction>(true);
        }
    }
}
