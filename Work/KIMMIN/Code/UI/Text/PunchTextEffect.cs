using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Work.Code.UI.Misc
{
    public class PunchTextEffect : MonoBehaviour, ITextEffect
    {
        [SerializeField] private float targetScale = 1.1f;
        [SerializeField] private float duration = 0.2f;
        
        private Vector3 _originScale;

        public void InitText(TextMeshProUGUI text)
        {
            _originScale = text.transform.localScale;
        }

        public void PlayEffect(TextMeshProUGUI text)
        {
            text.transform.DOKill();
            text.transform
                .DOScale(_originScale * targetScale, duration * 0.5f)
                .OnComplete(() =>
                {
                    text.transform.DOScale(_originScale, duration * 0.5f);
                });
        }
    }
}