using Code.UI.Core;
using Scripts.Combat.Datas;
using TMPro;
using UnityEngine;

namespace SHS.Scripts.Crosshairs
{
    public class VirtualCrosshair : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform crosshair;
        [SerializeField] private TextMeshProUGUI rangeText;

        [Header("Spread Visual (Optional)")]
        [SerializeField] private RectTransform[] spreadPoints;
        [SerializeField] private RectTransform spreadScaleTarget;
        [SerializeField] private float spreadPointMultiplier = 1f;
        [SerializeField] private float spreadScalePerPixel = 0.002f;

        private Vector2[] _spreadBasePositions;
        private Vector2 _screenPosition;
        private GunDataSO _gunData;

        // UI 기준 좌표/기준점 캐시를 준비한다.
        private void Awake()
        {
            if (!canvas)
                canvas = GetComponentInParent<Canvas>();

            CacheSpreadBasePositions();
            SetScreenPosition(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            SetSpreadRadiusPixels(0f);
        }
        
        public void SetRangeText(float distance)
        {
            if (rangeText == null) return;
            rangeText.text =  distance.ToString("0.0M");

            if (_gunData != null)
                rangeText.color = distance <= _gunData.attackRange ? Color.white : UIDefine.RedColor;
            else
                rangeText.color = Color.white;
        }

        // 크로스헤어 표시 여부를 토글한다.
        public void SetVisible(bool isVisible)
        {
            crosshair.gameObject.SetActive(isVisible);
        }

        // 스크린 좌표를 UI 좌표계로 변환해 루트 위치를 갱신한다.
        public void SetScreenPosition(Vector2 screenPx)
        {
            _screenPosition = screenPx;
            ApplyCrosshairUI(screenPx);
        }

        // 현재 스크린 좌표를 반환한다.
        public Vector2 GetCrosshairScreenPosition()
        {
            return _screenPosition;
        }

        // 전달받은 퍼짐 반경(픽셀)을 즉시 시각화한다.
        public void SetSpreadRadiusPixels(float spreadRadiusPixels)
        {
            ApplySpreadVisual(Mathf.Max(0f, spreadRadiusPixels));
        }

        // 캔버스 로컬 좌표로 루트 앵커 위치를 적용한다.
        private void ApplyCrosshairUI(Vector2 screenPx)
        {
            RectTransform canvasRect = (RectTransform)canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPx, null, out var localPosition);
            crosshair.anchoredPosition = localPosition;
        }

        // 포인트 분리형/스케일형 퍼짐 UI를 함께 갱신한다.
        private void ApplySpreadVisual(float spreadPixels)
        {
            float spreadOffset = spreadPixels * spreadPointMultiplier;

            if (spreadPoints != null && _spreadBasePositions != null)
            {
                int count = Mathf.Min(spreadPoints.Length, _spreadBasePositions.Length);
                for (int i = 0; i < count; i++)
                {
                    RectTransform point = spreadPoints[i];
                    if (point == null)
                        continue;

                    Vector2 basePos = _spreadBasePositions[i];
                    Vector2 direction = basePos.sqrMagnitude > 0.0001f ? basePos.normalized : Vector2.up;
                    point.anchoredPosition = basePos + direction * spreadOffset;
                }
            }

            if (spreadScaleTarget != null)
            {
                float scale = 1f + (spreadPixels * spreadScalePerPixel);
                spreadScaleTarget.localScale = new Vector3(scale, scale, 1f);
            }
        }

        // 퍼짐 포인트의 초기 위치를 기준값으로 저장한다.
        private void CacheSpreadBasePositions()
        {
            if (spreadPoints == null || spreadPoints.Length == 0)
            {
                _spreadBasePositions = null;
                return;
            }

            _spreadBasePositions = new Vector2[spreadPoints.Length];
            for (int i = 0; i < spreadPoints.Length; i++)
            {
                RectTransform point = spreadPoints[i];
                _spreadBasePositions[i] = point != null ? point.anchoredPosition : Vector2.zero;
            }
        }

        public void SetGunData(GunDataSO gunData)
        {
            _gunData = gunData;
        }
    }
}