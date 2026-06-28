using System;
using Code.UI.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Bar
{
    public class BarComponent : UIBase
    {
        [SerializeField] protected Image fill;
        [SerializeField] protected Image trailFill;
        [SerializeField] protected TextMeshProUGUI amountText;
        [SerializeField] protected float fillDuration = 0.12f;
        [SerializeField] protected float trailDelay = 0.15f;
        [SerializeField] protected float trailDuration = 0.25f;
        [SerializeField] private Ease fillEase = Ease.OutQuad;
        [SerializeField] private Ease trailEase = Ease.OutQuad;

        private Tween _fillTween;
        private Tween _trailTween;
        private float _target = -1f;
        private Color _fillColor;
        private Color _trailFillColor;

        protected override void Awake()
        {
            base.Awake();

            if (fill != null)
                _fillColor = fill.color;

            if (trailFill != null)
                _trailFillColor = trailFill.color;
        }

        public virtual void SetBar(float current, float max, Action<float> callback = null)
        {
            float target = max <= 0f ? 0f : Mathf.Clamp01(current / max);

            if (amountText != null)
                amountText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";

            if (fill == null)
                return;

            if (Mathf.Approximately(_target, target))
                return;

            _target = target;

            if (trailFill == null || trailFill == fill)
            {
                TweenFill(target, fillDuration, 0f, callback, current);
                return;
            }

            float fillScale = fill.transform.localScale.x;
            float trailScale = trailFill.transform.localScale.x;
            bool isFilling = target > fillScale;

            if (isFilling)
            {
                TweenFill(target, fillDuration, 0f, null, current);
                TweenTrail(target, fillDuration, 0f, callback, current);
                return;
            }

            if (trailScale < fillScale)
                SetScaleX(trailFill.transform, fillScale);

            TweenFill(target, fillDuration, 0f, null, current);
            TweenTrail(target, trailDuration, trailDelay, callback, current);
        }

        public void SetBarColor(Color color)
        {
            if (fill != null)
                fill.color = color;

            if (trailFill != null)
                trailFill.color = color;
        }

        public void ResetBarColor()
        {
            if (fill != null)
                fill.color = _fillColor;

            if (trailFill != null)
                trailFill.color = _trailFillColor;
        }

        private void TweenFill(float target, float duration, float delay, Action<float> callback, float current)
        {
            _fillTween?.Kill();
            _fillTween = TweenScaleX(fill.transform, target, duration, delay, fillEase)
                .OnComplete(() => callback?.Invoke(current));
        }

        private void TweenTrail(float target, float duration, float delay, Action<float> callback, float current)
        {
            _trailTween?.Kill();
            _trailTween = TweenScaleX(trailFill.transform, target, duration, delay, trailEase)
                .OnComplete(() => callback?.Invoke(current));
        }

        private Tween TweenScaleX(Transform targetTransform, float target, float duration, float delay, Ease ease)
        {
            if (duration <= 0f)
            {
                SetScaleX(targetTransform, target);
                return DOVirtual.DelayedCall(0f, () => { });
            }

            return targetTransform.DOScaleX(target, duration)
                .SetDelay(delay)
                .SetEase(ease)
                .SetUpdate(true);
        }

        private void SetScaleX(Transform targetTransform, float x)
        {
            Vector3 scale = targetTransform.localScale;
            scale.x = x;
            targetTransform.localScale = scale;
        }

        protected override void OnDestroy()
        {
            _fillTween?.Kill();
            _trailTween?.Kill();
            base.OnDestroy();
        }
    }
}
