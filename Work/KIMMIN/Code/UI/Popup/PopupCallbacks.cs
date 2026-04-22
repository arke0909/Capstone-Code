using System;

namespace Code.UI.Popup
{
    public interface ICallbackData { }
    
    public class EmptyCallback : ICallbackData { }

    public class ConfirmCallback : ICallbackData
    {
        public Action OnConfirm;
    }
    
    public class ConfirmCallback<T> : ICallbackData
    {
        public Action<T> OnConfirm;
    }
    
    public class ChoiceCallback : ICallbackData
    {
        public Action OnAccept;
        public Action OnReject;
    }
    
    public class ChoiceCallback<T> : ICallbackData
    {
        public Action<T> OnAccept;
        public Action<T> OnReject;
    }
}