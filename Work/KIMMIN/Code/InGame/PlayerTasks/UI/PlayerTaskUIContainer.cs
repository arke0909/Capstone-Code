using System.Collections.Generic;
using Code.UI.Core;
using DewmoLib.Dependencies;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.PlayerTasks.UI
{
    public class PlayerTaskUIContainer : UIBase
    {
        [Inject] private TaskController _taskController;
        [SerializeField] private RectTransform root;
        [SerializeField] private PlayerTaskUI taskUIPrefab;

        private readonly Queue<PlayerTaskUI> _taskUIPool = new();
        private readonly Dictionary<PlayerTask, PlayerTaskUI> _activeTaskUIs = new();

        private void Start()
        {
            _taskController.OnTaskAdded += HandleTaskAdded;
            _taskController.OnTaskCompleted += HandleTaskCompleted;
            _taskController.OnTaskRemoved += HandleTaskRemoved;
            _taskController.OnAllTasksCompleted += HandleAllTasksCompleted;

            foreach (PlayerTask task in _taskController.ActiveTasks)
            {
                AddTaskUI(task);
            }
            
            UpdateTaskUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _taskController.OnTaskAdded -= HandleTaskAdded;
            _taskController.OnTaskCompleted -= HandleTaskCompleted;
            _taskController.OnTaskRemoved -= HandleTaskRemoved;
            _taskController.OnAllTasksCompleted -= HandleAllTasksCompleted;
        }

        private void HandleAllTasksCompleted()
        {
            DisableUI();
        }

        private void HandleTaskCompleted(PlayerTask task)
        {
            if (!_activeTaskUIs.TryGetValue(task, out PlayerTaskUI ui))
                return;

            ui.CompleteTaskUI();
        }

        private void HandleTaskAdded(PlayerTask task)
        {
            AddTaskUI(task);
        }

        private void HandleTaskRemoved(PlayerTask task)
        {
            RemoveTaskUI(task);
        }

        private void AddTaskUI(PlayerTask task)
        {
            if (!task.ShowOnTaskUI)
                return;

            if (_activeTaskUIs.ContainsKey(task))
                return;

            PlayerTaskUI ui = GetTaskUI();
            ui.InitTaskUI(task);
            _activeTaskUIs.Add(task, ui);
            
            SortTaskUIs();
            UpdateTaskUI();
            ui.ShowTaskUI();
        }

        private void RemoveTaskUI(PlayerTask task)
        {
            if (!_activeTaskUIs.Remove(task, out PlayerTaskUI ui))
                return;

            ui.HideTaskUI(() =>
            {
                _taskUIPool.Enqueue(ui);
                UpdateTaskUI();
            });
        }

        private PlayerTaskUI GetTaskUI()
        {
            if (_taskUIPool.Count > 0)
            {
                return _taskUIPool.Dequeue();
            }

            PlayerTaskUI ui = Instantiate(taskUIPrefab, root);
            ui.DisableTaskUI();
            return ui;
        }

        private void UpdateTaskUI()
        {
            if(_activeTaskUIs.Count == 0 && IsActive)
                DisableUI();
            else if(_activeTaskUIs.Count > 0 && !IsActive)
                EnableUI();
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
        }

        private void SortTaskUIs()
        {
            List<PlayerTask> tasks = new List<PlayerTask>(_activeTaskUIs.Keys);
            tasks.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            for (int i = 0; i < tasks.Count; i++)
            {
                _activeTaskUIs[tasks[i]].transform.SetSiblingIndex(i);
            }
        }
    }
}
