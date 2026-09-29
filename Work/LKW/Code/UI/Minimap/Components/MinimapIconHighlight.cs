using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Minimap.Components
{
    /// <summary>
    /// 원본 아이콘의 알파 모양만 사용하는 단색 실루엣을 뒤에 배치하고 반복 강조한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MinimapIconHighlight : MonoBehaviour
    {
        private const string SilhouetteShaderResource = "MinimapSolidSilhouette";

        [Header("Color")]
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField, Range(0f, 1f)] private float minAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.9f;

        [Header("Pulse")]
        [SerializeField, Min(0.1f)] private float pulseDuration = 0.9f;
        [SerializeField, Min(1f)] private float minScale = 1.1f;
        [SerializeField, Min(1f)] private float maxScale = 1.35f;

        private static Material _silhouetteMaterial;

        private Image _sourceImage;
        private Image _silhouetteImage;
        private RectTransform _sourceRect;
        private RectTransform _silhouetteRect;
        private bool _isPlaying;

        private void Awake()
        {
            EnsureSilhouette();
            HideSilhouette();
        }

        private void OnEnable()
        {
            if (_isPlaying)
                ShowSilhouette();
        }

        private void OnDisable()
        {
            HideSilhouette();
        }

        private void LateUpdate()
        {
            if (!_isPlaying || _silhouetteImage == null) return;

            float duration = Mathf.Max(0.1f, pulseDuration);
            float phase = Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / duration) * 0.5f + 0.5f;
            float scale = Mathf.Lerp(minScale, maxScale, phase);

            Color color = highlightColor;
            color.a *= Mathf.Lerp(minAlpha, maxAlpha, phase);

            SyncSilhouetteTransform(scale);
            SyncSilhouetteImage();
            _silhouetteImage.color = color;
        }

        public void Play()
        {
            EnsureSilhouette();
            _isPlaying = true;
            ShowSilhouette();
        }

        public void Stop()
        {
            _isPlaying = false;
            HideSilhouette();
        }

        private void OnDestroy()
        {
            if (_silhouetteImage != null)
                Destroy(_silhouetteImage.gameObject);
        }

        private void EnsureSilhouette()
        {
            if (_silhouetteImage != null) return;
            if (!TryGetComponent(out _sourceImage)) return;

            Material material = GetSilhouetteMaterial();
            if (material == null) return;

            _sourceRect = _sourceImage.rectTransform;

            var silhouetteObject = new GameObject(
                $"{gameObject.name} Highlight",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            silhouetteObject.layer = gameObject.layer;
            _silhouetteImage = silhouetteObject.GetComponent<Image>();
            _silhouetteRect = _silhouetteImage.rectTransform;
            _silhouetteImage.material = material;
            _silhouetteImage.raycastTarget = false;

            SyncSilhouetteTransform(minScale);
            SyncSilhouetteImage();
        }

        private static Material GetSilhouetteMaterial()
        {
            if (_silhouetteMaterial != null) return _silhouetteMaterial;

            Shader shader = Resources.Load<Shader>(SilhouetteShaderResource);
            if (shader == null)
            {
                Debug.LogError($"Minimap highlight shader was not found in Resources: {SilhouetteShaderResource}");
                return null;
            }

            _silhouetteMaterial = new Material(shader)
            {
                name = "Minimap Solid Silhouette (Runtime)",
                hideFlags = HideFlags.HideAndDontSave
            };

            return _silhouetteMaterial;
        }

        private void SyncSilhouetteTransform(float scale)
        {
            if (_sourceRect == null || _silhouetteRect == null) return;

            Transform sourceParent = _sourceRect.parent;
            if (_silhouetteRect.parent != sourceParent)
                _silhouetteRect.SetParent(sourceParent, false);

            int sourceIndex = _sourceRect.GetSiblingIndex();
            int silhouetteIndex = _silhouetteRect.GetSiblingIndex();
            int targetIndex = silhouetteIndex < sourceIndex ? sourceIndex - 1 : sourceIndex;
            if (silhouetteIndex != targetIndex)
                _silhouetteRect.SetSiblingIndex(targetIndex);

            _silhouetteRect.anchorMin = _sourceRect.anchorMin;
            _silhouetteRect.anchorMax = _sourceRect.anchorMax;
            _silhouetteRect.pivot = _sourceRect.pivot;
            _silhouetteRect.anchoredPosition3D = _sourceRect.anchoredPosition3D;
            _silhouetteRect.sizeDelta = _sourceRect.sizeDelta;
            _silhouetteRect.localRotation = _sourceRect.localRotation;
            _silhouetteRect.localScale = _sourceRect.localScale * scale;
        }

        private void SyncSilhouetteImage()
        {
            if (_sourceImage == null || _silhouetteImage == null) return;

            _silhouetteImage.sprite = _sourceImage.sprite;
            _silhouetteImage.type = _sourceImage.type;
            _silhouetteImage.preserveAspect = _sourceImage.preserveAspect;
            _silhouetteImage.fillCenter = _sourceImage.fillCenter;
            _silhouetteImage.fillMethod = _sourceImage.fillMethod;
            _silhouetteImage.fillAmount = _sourceImage.fillAmount;
            _silhouetteImage.fillClockwise = _sourceImage.fillClockwise;
            _silhouetteImage.fillOrigin = _sourceImage.fillOrigin;
            _silhouetteImage.pixelsPerUnitMultiplier = _sourceImage.pixelsPerUnitMultiplier;
            _silhouetteImage.maskable = _sourceImage.maskable;
        }

        private void ShowSilhouette()
        {
            if (_silhouetteImage == null) return;

            SyncSilhouetteTransform(minScale);
            SyncSilhouetteImage();
            _silhouetteImage.gameObject.SetActive(true);
        }

        private void HideSilhouette()
        {
            if (_silhouetteImage != null)
                _silhouetteImage.gameObject.SetActive(false);
        }
    }
}
