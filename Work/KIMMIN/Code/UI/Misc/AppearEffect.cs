using DG.Tweening;
using UnityEngine;

namespace Work.Code.UI
{
    public class AppearEffect : MonoBehaviour
    {
        private Vector3 _originScale;

        private void Awake()
        {
            _originScale = transform.localScale;
        }

        public void Appear()
        {
            transform.DOKill();
            transform.DOScale(_originScale, 0.2f).SetEase(Ease.OutBack);
        }

        public void Disappear()
        {
            transform.DOKill();
            transform.DOScale(Vector3.zero, 0.16f).SetEase(Ease.InBack);
        }
    }
}