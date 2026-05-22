using System;
using UnityEngine;

namespace SHS.Scripts.UI
{
    public class TitleSceneBackground : MonoBehaviour
    {
        [SerializeField] private PlayerInputSO playerInputSO;
        [SerializeField] private Vector2 backgroundMoveOffsetRatio = new Vector2(1.1f, 1.1f);
        private Vector2 _startPosition;

        private void Awake()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            float currentMouseXRatio = (Mathf.Clamp01(playerInputSO.MouseScreenPosition.x / Screen.width) - 0.5f);
            float currentMouseYRatio = (Mathf.Clamp01(playerInputSO.MouseScreenPosition.y / Screen.height) - 0.5f);
            float offsetX = Screen.width * backgroundMoveOffsetRatio.x - Screen.width;
            float offsetY = Screen.height * backgroundMoveOffsetRatio.y - Screen.height;
            transform.position =
                _startPosition + new Vector2(currentMouseXRatio * offsetX, currentMouseYRatio * offsetY);
        }
    }
}