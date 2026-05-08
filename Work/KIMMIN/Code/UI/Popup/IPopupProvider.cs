using System;

namespace Code.UI.Popup
{
    public interface IPopupProvider
    {
        public event Action<Func<object>, ICallbackData> OnShowPopup;
    }
}