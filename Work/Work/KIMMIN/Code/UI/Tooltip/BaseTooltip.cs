using System;
using Code.UI.Core;

namespace Code.UI.Tooltip
{
    public abstract class BaseTooltip : LayoutUIBase
    {
        public override EUILayer Layer => EUILayer.Tooltip;
        public virtual int SortOrder => 0;
        public abstract Type DataType { get; }
        
        public abstract void ShowTooltip(object data);
        
        public virtual void HidePopup() => DisableUI(true);
    }

    public abstract class BaseTooltip<TData> : BaseTooltip
    {
        public override Type DataType => typeof(TData);
        
        public sealed override void ShowTooltip(object data)
        {
            EnableUI();
            ShowTooltip((TData)data);
        }
        
        protected abstract void ShowTooltip(TData data);
    }
}