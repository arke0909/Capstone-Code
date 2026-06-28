using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;
using Work.Code.PlayerTasks.TaskTrigger;

namespace Work.Code.PlayerTasks
{
    [Provide]
    public class TaskController : MonoBehaviour, IDependencyProvider
    {
        [SerializeField] private SoundID taskClearSound;
        [SerializeField] private float switchDelay = 0.2f;
        [SerializeField] private float removeDelay = 0.1f;

        [Inject] private Player _player;

        private readonly List<PlayerTask> _activeTasks = new();
        private readonly List<PlayerTask> _remainingTasks = new();
        private readonly List<List<PlayerTask>> _taskGroups = new();
        private readonly List<TaskCompleteInteraction[]> _completeInteractions = new();
        private readonly List<PlayerTask> _initializedTasks = new();

        private int _taskGroupIndex;
        private bool _isChangingGroup;

        public List<PlayerTask> ActiveTasks => _activeTasks;

        public event Action<PlayerTask> OnTaskAdded;
        public event Action<PlayerTask> OnTaskCompleted;
        public event Action<PlayerTask> OnTaskRemoved;
        public event Action OnAllTasksCompleted;

        private void Start()
        {
            InitTasks();
            SetTaskGroups();
            InitTaskTriggers();
            PlayTaskGroups();
        }

        private void OnDestroy()
        {
            DisposeTaskTriggers();
        }

        public void PlayTaskGroups()
        {
            _taskGroupIndex = 0;

            if (_taskGroups.Count == 0)
                return;

            StartTaskGroup();
        }

        public void AddTasksOnCurrentGroup(PlayerTask[] tasks)
        {
            List<PlayerTask> currentGroup = _taskGroups[_taskGroupIndex];

            foreach (PlayerTask task in tasks)
            {
                if (!currentGroup.Contains(task))
                    currentGroup.Add(task);

                if (!task.IsCompleted && !_remainingTasks.Contains(task))
                    _remainingTasks.Add(task);

                AddTask(task);
            }
        }

        private void RemoveTask(PlayerTask task)
        {
            if (!_activeTasks.Remove(task))
                return;

            task.OnTaskCompleted -= HandleTaskCompleted;
            task.CancelTask();
            OnTaskRemoved?.Invoke(task);
        }

        private bool AddTask(PlayerTask task)
        {
            if (_activeTasks.Contains(task))
                return false;

            InitTask(task);
            task.OnTaskCompleted += HandleTaskCompleted;
            _activeTasks.Add(task);
            OnTaskAdded?.Invoke(task);
            task.BeginTask();
            return true;
        }

        private void HandleTaskCompleted(PlayerTask task)
        {
            if (!_activeTasks.Contains(task))
                return;

            OnTaskCompleted?.Invoke(task);
            BroAudio.Play(taskClearSound);

            if (!_remainingTasks.Remove(task) || _remainingTasks.Count > 0)
                return;

            CompleteTaskGroup();
        }

        private void SetTaskGroups()
        {
            _taskGroups.Clear();
            _completeInteractions.Clear();

            TaskGroup[] taskGroups = GetComponentsInChildren<TaskGroup>(true);
            foreach (TaskGroup taskGroup in taskGroups)
            {
                AddTaskGroup(taskGroup.Tasks, taskGroup.CompleteInteractions);
            }

            foreach (Transform child in transform)
            {
                if (child.GetComponent<TaskGroup>() != null)
                    continue;

                if (child.GetComponentInChildren<TaskGroup>(true) != null)
                    continue;

                var tasks = child.GetComponentsInChildren<PlayerTask>(true);
                if (tasks.Length == 0)
                    continue;

                var interactions = child.GetComponentsInChildren<TaskCompleteInteraction>(true);
                AddTaskGroup(tasks, interactions);
            }
        }

        private void AddTaskGroup(PlayerTask[] tasks, TaskCompleteInteraction[] interactions)
        {
            List<PlayerTask> taskList = new List<PlayerTask>(tasks);
            taskList.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            _taskGroups.Add(taskList);
            _completeInteractions.Add(interactions);
        }

        private void StartTaskGroup()
        {
            PlayerTask[] tasks = _taskGroups[_taskGroupIndex].ToArray();
            _remainingTasks.Clear();

            foreach (PlayerTask task in tasks)
            {
                _remainingTasks.Add(task);
                AddTask(task);
            }

            if (_remainingTasks.Count == 0)
                CompleteTaskGroup();
        }

        private void CompleteTaskGroup()
        {
            if (_isChangingGroup)
                return;

            StartCoroutine(CompleteTaskGroupRoutine());
        }

        private IEnumerator CompleteTaskGroupRoutine()
        {
            _isChangingGroup = true;
            yield return ClearTaskGroup();
            _isChangingGroup = false;
            _taskGroupIndex++;

            if (_taskGroupIndex >= _taskGroups.Count)
            {
                _remainingTasks.Clear();
                OnAllTasksCompleted?.Invoke();
                yield break;
            }

            StartTaskGroup();
        }

        private IEnumerator ClearTaskGroup()
        {
            foreach (TaskCompleteInteraction interaction in _completeInteractions[_taskGroupIndex])
            {
                interaction.Interact();
            }

            List<PlayerTask> tasks = new List<PlayerTask>(_taskGroups[_taskGroupIndex]);
            tasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            for (int i = 0; i < tasks.Count; i++)
            {
                RemoveTask(tasks[i]);

                if (i < tasks.Count - 1)
                    yield return new WaitForSeconds(removeDelay);
            }

            yield return new WaitForSeconds(switchDelay);
        }

        private void InitTasks()
        {
            foreach (PlayerTask task in GetComponentsInChildren<PlayerTask>(true))
            {
                InitTask(task);
            }
        }

        private void InitTask(PlayerTask task)
        {
            if (_initializedTasks.Contains(task))
                return;

            task.InitializeTask(_player);
            _initializedTasks.Add(task);
        }

        private void InitTaskTriggers()
        {
            foreach (PlayerTaskTrigger trigger in GetComponentsInChildren<PlayerTaskTrigger>(true))
            {
                trigger.InitTaskTrigger(_player, this);
            }
        }

        private void DisposeTaskTriggers()
        {
            foreach (PlayerTaskTrigger trigger in GetComponentsInChildren<PlayerTaskTrigger>(true))
            {
                trigger.DisposeTaskTrigger();
            }
        }
    }
}
