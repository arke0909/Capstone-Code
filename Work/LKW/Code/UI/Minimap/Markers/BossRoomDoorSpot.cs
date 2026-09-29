using Code.UI.Minimap.Core;
using UnityEngine;

namespace Code.UI.Minimap.Markers
{
    public class BossRoomDoorSpot : MonoBehaviour
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private bool syncChildScale = true;
        [SerializeField] private ElementType elementType = ElementType.LockedMarker;

        private string _minimapId;

        private void Start()
        {
            _minimapId = MinimapUtil.AddToMinimap(
                this,
                elementType,
                icon,
                syncChildScale,
                transform.position);
        }

        private void OnDestroy()
        {
            _minimapId.RemoveFromMinimap();
        }
    }
}
