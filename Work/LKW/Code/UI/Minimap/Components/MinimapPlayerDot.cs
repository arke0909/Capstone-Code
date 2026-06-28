using Code.UI.Minimap;
using DewmoLib.Dependencies;
using Scripts.Players;
using UnityEngine;

namespace Code.UI.Minimap.Components
{
    public class MinimapPlayerDot : MonoBehaviour
    {
        [Inject] private Player _player;
        [SerializeField] private MinimapUI minimapUI;
        [SerializeField] private bool isFollow = false;
        [field: SerializeField] public RectTransform PlayerDot { get; private set; }
        [SerializeField] private RectTransform directionImage;
        [SerializeField] private Vector2 directionOffset = new Vector2(0f, 20f);

        private void Awake()
        {
            if (directionImage == null && PlayerDot != null && PlayerDot.childCount == 1)
                directionImage = PlayerDot.GetChild(0) as RectTransform;
        }

        private void Update()
        {
            if (_player == null || minimapUI.MinimapSystem == null || PlayerDot == null) return;

            Vector2 normalizedPos = minimapUI.MinimapSystem.WorldToNormalizedPosition(_player.transform.position);

            PlayerDot.anchoredPosition = new Vector2(
                (normalizedPos.x - 0.5f) * minimapUI.MiniMapRect.sizeDelta.x,
                (normalizedPos.y - 0.5f) * minimapUI.MiniMapRect.sizeDelta.y
            );

            float playerRotation = _player.transform.eulerAngles.y;
            if (directionImage != null)
            {
                Quaternion directionRotation = Quaternion.Euler(0, 0, -playerRotation);
                directionImage.anchoredPosition = directionRotation * directionOffset;
                directionImage.localRotation = Quaternion.Euler(0, 0, -playerRotation);
            }
            
            if(isFollow)
                minimapUI.MiniMapRect.anchoredPosition = -PlayerDot.anchoredPosition;
        }
    }
}
