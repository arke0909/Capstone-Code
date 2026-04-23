using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Work.Code.Effect
{
    public class PlayerHitVignette : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        [SerializeField] private float intensityTarget = 0.4f;
        [SerializeField] private float duration = 0.1f;

        private Tween _tween;
        
        public void PlayVignette()
        {
            _tween?.Kill();
            volume.weight = 0;
            
            _tween = DOTween.To(() => volume.weight, 
                    x => volume.weight = x, intensityTarget, duration)
                .SetEase(Ease.OutQuart)
                .SetLoops(2, LoopType.Yoyo);
        }
    }
}