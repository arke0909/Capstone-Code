using Code.UI.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Work.Code.UI.Interaction
{
    public class InteractVisualUI : UIBase
    {
        [SerializeField] private Image outlineCircle;

        private Sequence _highlightSeq;
        private Vector3 _originalScale;

        protected override void Awake()
        {
            base.Awake();
            
            _originalScale = outlineCircle.transform.localScale;
            float scale = _originalScale.x * 1.3f;
            
            _highlightSeq = DOTween.Sequence();
            _highlightSeq.Append(outlineCircle.transform.DOScale(scale, 0.5f));
            _highlightSeq.Join(outlineCircle.DOFade(0, 1f).SetEase(Ease.InSine));
            _highlightSeq.SetLoops(-1, LoopType.Restart);
            _highlightSeq.SetAutoKill(false);
            _highlightSeq.Pause();
        }

        public void PlayHighlight()
        {
            _highlightSeq.Play();
            ResetUI();
        }

        public void StopHighlight()
        {
            _highlightSeq.Pause();
            ResetUI();
        }

        private void ResetUI()
        {
            var c = outlineCircle.color;
            outlineCircle.color = new Color(c.r, c.g, c.b, 1f);
            outlineCircle.transform.localScale = _originalScale;
        }
    }
}