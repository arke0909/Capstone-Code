using Code.TimeSystem;
using Code.UI.Minimap.Core;

namespace Code.UI.Minimap.Markers
{
    public class SupplyIcon : MinimapElement
    {
        public void SetLifeTimer() => TimeController.Instance.AddEvent(TimeUtil.Day(0.5f), RemoveSelf);
    }
}