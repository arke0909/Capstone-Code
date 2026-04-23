using System;
using Code.UI.Core.Interaction;
using Work.Code.UI.Core.Interaction;

namespace Code.UI.Popup
{
    public interface IPopupable : IClickable
    {
        public event Action<Func<object>, ICallbackData> OnClickHandler;
    }
}