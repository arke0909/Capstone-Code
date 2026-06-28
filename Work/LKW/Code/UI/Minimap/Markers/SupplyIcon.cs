using Code.TimeSystem;
using Code.UI.Minimap.Core;

namespace Code.UI.Minimap.Markers
{
    public class SupplyIcon : MinimapElement
    {
        public void SetLifeTimer()
        {
            string targetId = ID;
            TimeController.Instance.AddEvent(TimeUtil.Day(0.5f), () =>
            {
                if (ID == targetId)
                    RemoveSelf();
            });
        }
    }
}
