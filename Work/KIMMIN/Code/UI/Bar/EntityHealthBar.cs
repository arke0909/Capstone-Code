using System;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Entities.Enemies.Events.Local;
using DG.Tweening;
using Scripts.Combat.Fovs;
using Scripts.Entities;
using Scripts.Entities.Vitals;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Bar
{
    public struct VisibleStateChangeEvent : ILocalEvent
    {
        public VisibleState VisibleState { get; }
        public bool IsVisible { get; } //현재 보이는 상태인지 ex)InFov
        public bool IsFound { get; } //현재 Fov내에 노출된 상태인지 ex) Stealth but SightCount > 0
        public VisibleStateChangeEvent(VisibleState visibleState,bool isVisible,bool isFound)
        {
            VisibleState = visibleState;
            IsVisible = isVisible;
            IsFound = isFound;
        }
    }
    public class EntityHealthBar : BarComponent, IContainerComponent, IAfterInitialze, ILocalEventSubscriber<VisibleStateChangeEvent>,ILocalEventSubscriber<HealthShieldChangeEvent>, ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField] private RectTransform shieldFill;

        private Camera _cam;
        private LocalEventBus _localEventBus;
        public ComponentContainer ComponentContainer { get; set; }
        public void OnInitialize(ComponentContainer componentContainer)
        {
            _cam = Camera.main;
            _localEventBus = componentContainer.Get<LocalEventBus>();
        }

        public void AfterInitialize()
        {
            _localEventBus.Raise(new HealthShieldStateRequestEvent());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        public void OnLocalEvent(VisibleStateChangeEvent @event)
        {
            gameObject.SetActive(@event.IsVisible);
        }
        public void OnLocalEvent(HealthShieldChangeEvent @event)
        {
            SetBar(@event.CurrentHealth, @event.MaxHealth, HandleAfterEffect);
            SetShield(@event.CurrentHealth, @event.MaxHealth, @event.CurrentShield);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            EnableUI(true);
            _localEventBus.Raise(new HealthShieldStateRequestEvent());
        }

        private void HandleAfterEffect(float current)
        {
            if (current <= 0f)
                DisableUI(true);
        }

        private void SetShield(float currentHealth, float maxHealth, float currentShield)
        {
            float healthRatio = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
            float shieldRatio = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentShield / maxHealth);
            float shieldEnd = Mathf.Clamp01(healthRatio + shieldRatio);
            float shieldStart = Mathf.Max(0f, shieldEnd - shieldRatio);

            bool hasShield = currentShield > 0f && maxHealth > 0f;
            shieldFill.gameObject.SetActive(hasShield);
            shieldFill.anchorMin = new Vector2(shieldStart, 0f);
            shieldFill.anchorMax = new Vector2(shieldEnd, 1f);
            shieldFill.offsetMin = Vector2.zero;
            shieldFill.offsetMax = Vector2.zero;
        }

        private void LateUpdate()
        {
            if (!_cam) return;
            transform.forward = _cam.transform.forward;
        }

        public override void SetBar(float current, float max, Action<float> callback = null)
        {
            float target = max <= 0f ? 0f : Mathf.Clamp01(current / max);

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
