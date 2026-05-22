using Chipmunk.GameEvents;
using Code.GameEvents;
using DG.Tweening;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class ModifyUIOnPlayerUI : MonoBehaviour
    {
        [SerializeField] private RectTransform uiHolder;
        [SerializeField] private Vector2 moveScale;
        
        private Vector3 _originalPosition;

        private void Awake()
        {
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUI);
            _originalPosition = uiHolder.anchoredPosition;
        }
        
        private void HandlePlayerUI(PlayerUIEvent evt)
        {
            uiHolder.DOKill();
            
            Vector2 target = evt.IsEnabled ? moveScale : _originalPosition;
            uiHolder.DOAnchorPos(target, 0.25f).SetEase(Ease.OutQuart);
        }
    }
}