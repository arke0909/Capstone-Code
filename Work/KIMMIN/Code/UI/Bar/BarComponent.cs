using System;
using Code.UI.Core;
using DG.Tweening;
using Scripts.Entities.Vitals;
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

        private Tween _trailTween;

        
        public virtual void SetBar(float current, float max,  Action<float> callback = null)
        {
            float target = current / max;
            
            if (amountText != null)
                amountText.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
            
            fill.transform.localScale = new Vector3(target, 1, 1);
            _trailTween?.Kill();
            _trailTween = DOVirtual.DelayedCall(0.2f, () =>
            {
                trailFill.transform.DOScaleX(target, 0.4f).SetUpdate(true);
            });
        }
    }
}