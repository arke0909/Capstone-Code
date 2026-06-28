using Scripts.Players;
using UnityEngine;

namespace Work.Code.PlayerTasks.TaskTrigger
{
    public abstract class PlayerTaskTrigger : MonoBehaviour
    {
        protected TaskController _taskController;
        protected Player _player;
        protected PlayerTask[] _tasks;

        public void InitTaskTrigger(Player owner, TaskController taskController)
        {
            _player = owner;
            _taskController = taskController;
            _tasks = GetComponentsInChildren<PlayerTask>(true);
            OnInitTaskTrigger();
        }

        public void DisposeTaskTrigger()
        {
            OnDisposeTaskTrigger();
        }

        protected abstract void OnInitTaskTrigger();
        protected abstract void OnDisposeTaskTrigger();

        protected void RaisePlayerTask()
        {
            _taskController.AddTasksOnCurrentGroup(_tasks);
        }

        protected void RaisePlayerTask(PlayerTask[] tasks)
        {
            _taskController.AddTasksOnCurrentGroup(tasks);
        }
    }
}
