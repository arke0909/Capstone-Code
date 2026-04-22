using System;
using Code.UI.Core;

namespace Code.UI.Popup
{
    public abstract class BasePopup : UIBase
    {
        public override EUILayer Layer => EUILayer.Popup;
        public abstract Type DataType { get; }
        
        public abstract void ShowPopup(object data, ICallbackData callback = null);
        
        public virtual void ClosePopup() => DisableUI(true);
    }
    
    public abstract class BasePopup<TData, TCallback> : BasePopup where TCallback : ICallbackData
    {
        protected TCallback _callback;
        public override Type DataType => typeof(TData);

        public sealed override void ShowPopup(object data, ICallbackData callback = null)
        {
            _callback = (TCallback)callback;
            ShowPopup((TData)data, (TCallback)callback);
        }

        public override void ClosePopup()
        {
            base.ClosePopup();
            _callback = default;
        }
        
        protected abstract void ShowPopup(TData data, TCallback callback);
    }
}