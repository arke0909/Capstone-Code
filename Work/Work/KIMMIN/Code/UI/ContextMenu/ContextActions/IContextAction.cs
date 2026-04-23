using System;

namespace Work.Code.UI.ContextMenu
{
    public interface IContextAction<T>
    { 
        public event Action<T> OnContextAction; 
        public event Action OnCallbackInvoked;
        public ContextActionSO ContextActionSO { get; }
        public void Init(T data);
        public bool CheckCondition(T data);
        public void OnAction(T data);
        public bool CanShow(T data);
    }
}