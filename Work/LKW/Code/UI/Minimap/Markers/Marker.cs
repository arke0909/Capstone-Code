using Code.UI.Minimap.Core;

namespace Code.UI.Minimap.Markers
{
    public class Marker : MinimapElement
    {
        public bool CanRemove { get; set; } = true;

        public override void Initialize(MinimapElementData data)
        {
            base.Initialize(data);
            CanRemove = true;
        }
    }
}
