using Chipmunk.GameEvents;
using Code.Items.ItemInfo;
using Code.UI.Minimap.Core;
using UnityEngine;
using Work.Code.GameEvents;

namespace Code.UI.Minimap.Markers
{
    public class ItemRevealedMinimapSpot : MonoBehaviour
    {
        [SerializeField] private Sprite icon;
        [SerializeField] private bool syncChildScale = true;
        [SerializeField] private ElementType elementType = ElementType.LockedMarker;
        [SerializeField] private ItemDataSO requiredItem;
        [SerializeField] private string revealMessage = "보트의 위치가 표시되었습니다";

        private string _minimapId;

        private void OnEnable()
        {
            EventBus.Subscribe<ItemPickedUpEvent>(HandleItemPickedUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ItemPickedUpEvent>(HandleItemPickedUp);
        }

        private void OnDestroy()
        {
            _minimapId.RemoveFromMinimap();
        }

        private void HandleItemPickedUp(ItemPickedUpEvent evt)
        {
            if (requiredItem == null || evt.ItemData != requiredItem || !string.IsNullOrEmpty(_minimapId))
                return;

            _minimapId = MinimapUtil.AddToMinimap(
                this,
                elementType,
                icon,
                syncChildScale,
                transform.position,
                isHighlighted: true);

            EventBus.Raise(new SystemMessageEvent(revealMessage));
        }
    }
}
