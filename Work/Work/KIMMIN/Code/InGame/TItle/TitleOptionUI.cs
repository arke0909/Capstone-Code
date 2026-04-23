using System;
using Code.UI.Core;
using Code.UI.Core.Interaction;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Work.Code.Setting
{
    public class TitleOptionUI : InteractableUI, IHoverable
    {
        [SerializeField] private Image background;
        [SerializeField] private RectTransform rect;

        private readonly Vector3 _highlightSize = new(275, 125);
        private readonly Vector3 _normalSize = new(250, 100);
        private Tween _sizeTween;
        
        public void OnHoverEnter(PointerEventData eventData)
        {
            _sizeTween?.Kill();
            _sizeTween = rect.DOSizeDelta(_highlightSize, 0.1f).SetUpdate(true);
        }

        public void OnHoverExit(PointerEventData eventData)
        {
            _sizeTween?.Kill();
            _sizeTween = rect.DOSizeDelta(_normalSize, 0.1f).SetUpdate(true);
        }
    }
}