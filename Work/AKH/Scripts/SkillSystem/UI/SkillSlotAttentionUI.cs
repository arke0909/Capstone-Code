using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.SkillSystem.UI
{
    public sealed class SkillSlotAttentionUI : IDisposable
    {
        private const float PulseDuration = 0.8f;
        private static readonly Color AttentionBackgroundColor = new(0.35f, 0.12f, 0f, 0.72f);
        private static readonly Color AttentionOutlineColor = new(1f, 0.49f, 0.05f, 1f);

        private readonly Image _background;
        private readonly Image _outline;
        private readonly GameObject _attentionMark;
        private readonly Color _defaultBackgroundColor;
        private readonly Color _defaultOutlineColor;
        private Sequence _pulseSequence;

        public SkillSlotAttentionUI(Transform slotRoot)
        {
            _background = slotRoot.GetComponent<Image>();
            _outline = slotRoot.Find("outline")?.GetComponent<Image>();
            _attentionMark = slotRoot.Find("AttentionMark")?.gameObject;

            if (_background == null || _outline == null || _attentionMark == null)
                throw new MissingReferenceException(
                    $"{slotRoot.name} requires root/outline Images and an AttentionMark child.");

            _defaultBackgroundColor = _background.color;
            _defaultOutlineColor = _outline.color;
        }

        public void PlayAttention()
        {
            StopAttention();
            _attentionMark.SetActive(true);

            float halfDuration = PulseDuration * 0.5f;
            _pulseSequence = DOTween.Sequence()
                .Append(_background.DOColor(AttentionBackgroundColor, halfDuration).SetEase(Ease.InOutSine))
                .Join(_outline.DOColor(AttentionOutlineColor, halfDuration).SetEase(Ease.InOutSine))
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        public void StopAttention()
        {
            _pulseSequence?.Kill();
            _pulseSequence = null;
            _attentionMark.SetActive(false);
            _background.color = _defaultBackgroundColor;
            _outline.color = _defaultOutlineColor;
        }

        public void Dispose()
        {
            _pulseSequence?.Kill();
            _attentionMark.SetActive(false);
        }
    }
}
