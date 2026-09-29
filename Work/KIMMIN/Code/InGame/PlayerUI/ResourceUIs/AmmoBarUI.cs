using Code.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies.Events.Local;
using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.PlayerUI.ResourceUIs
{
    public class AmmoBarUI : UIBase, ILocalEventSubscriber<AmmoUpdateEvent>,
        ILocalEventSubscriber<EnemySpawnEvent>
    {
        [SerializeField] private Image fill;
        [SerializeField] private Color normalColor = new Color(0.957f, 0.776f, 0.420f);
        [SerializeField] private Color lowAmmoColor = new Color(1f, 0.608f, 0.439f);
        [SerializeField, Range(0f, 1f)] private float lowAmmoThreshold = 0.2f;

        private float _fillAmount = 1f;
        private bool _hasCapacity;

        protected override void Awake()
        {
            IsActive = false;
            base.Awake();
            ApplyAmmo(false);
        }

        private void OnEnable()
        {
            ApplyAmmo(false);
        }

        public void OnLocalEvent(AmmoUpdateEvent eventData)
        {
            // Updates can arrive before Awake or while the parent HP bar is hidden.
            _hasCapacity = eventData.TotalAmmo > 0;
            _fillAmount = _hasCapacity
                ? Mathf.Clamp01((float)eventData.CurrentAmmo / eventData.TotalAmmo)
                : 0f;
            ApplyAmmo(isActiveAndEnabled);
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            _hasCapacity = false;
            _fillAmount = 1f;
            ApplyAmmo(false);
        }

        private void ApplyAmmo(bool fade)
        {
            if (CanvasGroup == null)
                return;

            fill.fillAmount = _fillAmount;
            fill.color = _fillAmount <= lowAmmoThreshold ? lowAmmoColor : normalColor;

            if (_hasCapacity && _fillAmount < 1f)
                EnableUI(fade);
            else
                DisableUI(fade);
        }
    }
}
