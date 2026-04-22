using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Work.Code.UI.Misc
{
    public class ColorTextEffect : MonoBehaviour, ITextEffect
    {
        [SerializeField] private Color targetColor;
        [SerializeField] private float duration;
        
        public void InitText(TextMeshProUGUI text)
        {
            targetColor = text.color;
        }

        public void PlayEffect(TextMeshProUGUI text)
        {
            text.DOColor(targetColor, duration).OnComplete(() =>
            {
                text.DOColor(targetColor, duration);
            });
        }
    }
}