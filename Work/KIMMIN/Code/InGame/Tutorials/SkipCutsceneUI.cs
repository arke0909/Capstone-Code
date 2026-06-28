using Code.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.Tutorials
{
    public class SkipCutsceneUI : UIBase
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI skipText;

        private const float ResetDuration = 0.12f;

        private Tween _fillTween;

        protected override void Awake()
        {
            base.Awake();
            SetProgressImmediately(0f);
            DisableUI();
        }

        public override void DisableUI(bool isFade = false)
        {
            base.DisableUI(isFade);
            skipText.gameObject.SetActive(false);
        }

        public override void EnableUI(bool isFade = false)
        {
            base.EnableUI(isFade);
            skipText.gameObject.SetActive(true);
        }

        public void StartProgress(float duration)
        {
            _fillTween?.Kill();
            _fillTween = fillImage.transform.DOScaleX(1f, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }

        public void ResetProgress()
        {
            _fillTween?.Kill();
            _fillTween = fillImage.transform.DOScaleX(0f, ResetDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void SetProgressImmediately(float progress)
        {
            _fillTween?.Kill();

            Vector3 scale = fillImage.transform.localScale;
            scale.x = Mathf.Clamp01(progress);
            fillImage.transform.localScale = scale;
        }

        protected override void OnDestroy()
        {
            _fillTween?.Kill();
            base.OnDestroy();
        }
    }
}
