using Chipmunk.GameEvents;
using DG.Tweening;
using InGame.PlayerUI;
using UnityEngine;

namespace Code.UI.Core
{
    public class CursorManager : MonoBehaviour
    {
        private CursorLockMode _currentMode = CursorLockMode.None;

        private void Awake()
        {
            DOTween.SetTweensCapacity(500, 100);
            UIManager.Instance.OnUIStackChanged += HandleChangeUIStack;
            Cursor.lockState = _currentMode = CursorLockMode.Locked;
        }

        private void OnDestroy()
        {
            UIManager.Instance.OnUIStackChanged -= HandleChangeUIStack;
        }

        private void HandleChangeUIStack()
        {
            SetCursor();
        }
        
        private void SetCursor()
        {
            var newMode = UIManager.Instance.HasStackUI() ? CursorLockMode.None : CursorLockMode.Locked;
            
            if (_currentMode == newMode) return;
            Cursor.lockState = _currentMode = newMode;
            EventBus.Raise(new ChangeCursorEvent(newMode == CursorLockMode.Locked));
        }
    }
}