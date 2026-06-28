using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.Code.PlayerTasks
{
    public class KeyPressTask : PlayerTask, IProgressTask
    {
        [SerializeField] private InputAction keyAction;
        [SerializeField] private float holdTime;
        [SerializeField] private string overrideKeyString;
        [SerializeField] private string suffix = "키를 누르세요.";

        private float _heldTime;
        private bool _isPressed;
        private bool _completeHolding;

        public bool HasProgress => holdTime > 0f;

        public float Progress
        {
            get
            {
                if (_completeHolding)
                    return 1f;

                if (holdTime <= 0f)
                    return 1f;

                return Mathf.Clamp01(_heldTime / holdTime);
            }
        }

        public override void StartTask()
        {
            base.StartTask();
            
            _heldTime = 0f;
            _isPressed = false;
            _completeHolding = false;

            keyAction.started  += HandleKeyStarted;
            keyAction.canceled += HandleKeyCanceled;
            keyAction.Enable();
        }
        
        private void Update()
        {
            if (!_isPressed) return;

            _heldTime += Time.deltaTime;

            if (_heldTime >= holdTime)
            {
                _heldTime = holdTime;
                _isPressed = false;
                CompleteTask();
            }
        }
        
        private void HandleKeyStarted(InputAction.CallbackContext ctx)
        {
            _isPressed = true;
            _heldTime = 0f;

            if (holdTime <= 0f)
            {
                _heldTime = 1f;
                CompleteTask();
            }
        }

        private void HandleKeyCanceled(InputAction.CallbackContext ctx)
        {
            if (_completeHolding)
                return;

            _isPressed = false;
            _heldTime = 0f;
        }

        protected override void StopTask()
        {
            _isPressed = false;
            keyAction.started  -= HandleKeyStarted;
            keyAction.canceled -= HandleKeyCanceled;
            keyAction.Disable();
        }

        protected override void CompleteTask()
        {
            if (_completeHolding)
                return;

            _completeHolding = true;
            base.CompleteTask();
        }

        protected override string GetTaskText()
        {
            return overrideKeyString == string.Empty ?
                $"{keyAction.GetBindingDisplayString()}{suffix}" : 
                $"{overrideKeyString}{suffix}";
        }
    }
}
