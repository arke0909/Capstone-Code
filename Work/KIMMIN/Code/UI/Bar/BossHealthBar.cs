using Chipmunk.GameEvents;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Events;
using Scripts.Entities.Vitals;
using TMPro;
using UnityEngine;

namespace Code.UI.Bar
{
    public class BossHealthBar : BarComponent
    {
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private RectTransform shieldFill;
        private CanvasGroup canvasGroup;
        private LocalEventBus drawingEventBus;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("<color=green>BossHealthBar Awake</color>");
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            EventBus<BossCombatEnteredEvent>.OnEvent += HandleBossCombatEntered;
            HandleBossCombatEntered(new BossCombatEnteredEvent(null));
        }

        protected override void OnDestroy()
        {
            UnbindHealthComponent();
            base.OnDestroy();
            EventBus<BossCombatEnteredEvent>.OnEvent -= HandleBossCombatEntered;
        }

        private void HandleBossCombatEntered(BossCombatEnteredEvent evt)
        {
            UnbindHealthComponent();

            Boss boss = evt.Boss;
            if (boss == null)
            {
                Hide();
                return;
            }

            drawingEventBus = boss.LocalEventBus;
            drawingEventBus.Subscribe<HealthShieldChangeEvent>(HandleHealthShieldChanged);

            if (boss.EnemyData == null)
                SetNameText("Umm : Unknown : ");
            else
                SetNameText(boss.EnemyData.name);

            Show();
            drawingEventBus.Raise(new HealthShieldStateRequestEvent());
        }

        private void HandleHealthShieldChanged(HealthShieldChangeEvent evt)
        {
            SetBar(evt.CurrentHealth, evt.MaxHealth);
            SetShield(evt.CurrentHealth, evt.MaxHealth, evt.CurrentShield);

            int roundedHealth = Mathf.RoundToInt(evt.CurrentHealth);
            int roundedMaxHealth = Mathf.RoundToInt(evt.MaxHealth);
            int roundedShield = Mathf.RoundToInt(evt.CurrentShield);
            healthText.text = evt.CurrentShield > 0f
                ? $"{roundedHealth} (+{roundedShield}) / {roundedMaxHealth}"
                : $"{roundedHealth} / {roundedMaxHealth}";

            if (evt.CurrentHealth <= 0f)
            {
                Hide();
                UnbindHealthComponent();
            }
        }

        private void UnbindHealthComponent()
        {
            if (drawingEventBus == null)
                return;

            drawingEventBus.Unsubscribe<HealthShieldChangeEvent>(HandleHealthShieldChanged);
            drawingEventBus = null;
            shieldFill.gameObject.SetActive(false);
        }

        private void SetShield(float currentHealth, float maxHealth, float currentShield)
        {
            float healthRatio = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
            float shieldRatio = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentShield / maxHealth);
            float shieldEnd = Mathf.Clamp01(healthRatio + shieldRatio);
            float shieldStart = Mathf.Max(0f, shieldEnd - shieldRatio);

            shieldFill.gameObject.SetActive(currentShield > 0f && maxHealth > 0f);
            shieldFill.anchorMin = new Vector2(shieldStart, 0f);
            shieldFill.anchorMax = new Vector2(shieldEnd, 1f);
            shieldFill.offsetMin = Vector2.zero;
            shieldFill.offsetMax = Vector2.zero;
        }

        public void SetNameText(string text) => bossNameText.text = text;

        public void Show()
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
