using System;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using DG.Tweening;
using Scripts.Entities.Vitals;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Bar
{
    public class EntityHealthBar : BarComponent, IContainerComponent
    {
        [SerializeField] private float fillDuration = 0.05f;
        [SerializeField] private float trailDelay = 0.2f;
        [SerializeField] private float trailDuration = 0.3f;
        
        private Camera _cam;
        private LocalEventBus _localEventBus;
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _cam = Camera.main;
            _localEventBus = componentContainer.Get<LocalEventBus>();
            _localEventBus.Subscribe<HealthChangeEvent>(HandleHealthChanged);
        }
        
        private void OnDestroy()
        {
            _localEventBus.Unsubscribe<HealthChangeEvent>(HandleHealthChanged);
        }
        private void HandleHealthChanged(HealthChangeEvent @event)
        {
            SetBar(@event.CurrentHealth, @event.MaxHealth, HandleAfterEffect);
        }

        private void HandleAfterEffect(float current)
        {
            if (current <= 0)
                DisableUI(true);
        }

        private void LateUpdate()
        {
            if (!_cam) return;
            transform.forward = _cam.transform.forward;
        }

        public override void SetBar(float current, float max, Action<float> callback = null)
        {
            float target = current / max;

            fill.DOKill();
            trailFill.DOKill();

            if (target < fill.fillAmount)
            {
                trailFill.gameObject.SetActive(true);
                fill.DOFillAmount(target, fillDuration);
                trailFill.fillAmount = fill.fillAmount;
                trailFill.DOFillAmount(target, trailDuration).SetDelay(trailDelay)
                    .OnComplete(() =>
                    {
                        callback?.Invoke(current);
                        trailFill.gameObject.SetActive(false);
                    });
            }
            else
            {
                fill.DOFillAmount(target, fillDuration);
                trailFill.DOFillAmount(target, fillDuration);
            }
        }
    }
}