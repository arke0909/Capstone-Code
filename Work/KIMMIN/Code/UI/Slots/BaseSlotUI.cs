using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Interaction;

namespace Work.Code.UI.Slots
{
    public class BaseSlotUI : DraggableUI
    {
        [SerializeField] protected Image outline;
        [SerializeField] protected Image backgroundEffect;
        
        private Sequence _backgroundEffectSeq;

        public void PlayBackgroundEffect(Color effectColor)
        {
            StopBackgroundEffect();
            _backgroundEffectSeq = DOTween.Sequence();
            backgroundEffect.color = effectColor;
            
            _backgroundEffectSeq.Append(backgroundEffect.transform.DOScale(1.3f, 1f))
                .SetEase(Ease.OutCirc);
            _backgroundEffectSeq.Join(backgroundEffect.DOFade(0f, 1f))
                .SetEase(Ease.OutCirc);
            _backgroundEffectSeq.SetLoops(-1, LoopType.Restart);
        }

        public void StopBackgroundEffect()
        {
            backgroundEffect.transform.localScale = Vector3.one;
            backgroundEffect.color = new Color(0f, 0f, 0f, 0f);
            _backgroundEffectSeq?.Kill();
        }
    }
}