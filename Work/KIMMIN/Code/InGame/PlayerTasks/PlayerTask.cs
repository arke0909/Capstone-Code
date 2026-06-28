using System;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.PlayerTasks
{
    public abstract class PlayerTask : MonoBehaviour
    {
        protected Player _player;
        private bool _isRunning;
        private bool _isCompleted;
        
        [field: SerializeField] public int Priority { get; private set; }
        [field: SerializeField] public string TaskId { get; private set; } 
        public string TaskText { get; private set; }
        public bool IsCompleted => _isCompleted;
        public virtual bool ShowOnTaskUI => true;
            
        public event Action<PlayerTask> OnTaskCompleted;
        public event Action<PlayerTask> OnTaskTextChanged;
        
        public virtual void InitializeTask(Player player)
        {
            _player = player;
        }

        public void BeginTask()
        {
            _isRunning = true;
            _isCompleted = false;
            UpdateTaskText();
            StartTask();
        }

        public virtual void StartTask()
        {
            UpdateTaskText();
        }

        public void CancelTask()
        {
            if (!_isRunning || _isCompleted)
                return;

            _isRunning = false;
            StopTask();
        }

        protected abstract void StopTask();

        protected void UpdateTaskText()
        {
            TaskText = GetTaskText();
            OnTaskTextChanged?.Invoke(this);
        }

        protected virtual void CompleteTask()
        {
            if (_isCompleted)
                return;

            _isCompleted = true;
            _isRunning = false;
            OnTaskCompleted?.Invoke(this);
            StopTask();
        }
        
        protected abstract string GetTaskText();
    }
}
