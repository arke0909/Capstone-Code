using System;
using Assets.Work.AKH.Scripts.Entities.Vitals;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.Events;
using TMPro;
using UnityEngine;

namespace Code.UI.Bar
{
    public class BossHealthBar : BarComponent
    {
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI bossNameText;
        private CanvasGroup canvasGroup;
        private HealthCompo drawingHealthCompo;

        private void Awake()
        {
            Debug.Log("<color=green>BossHealthBar Awake</color>");
            ;
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            EventBus<BossCombatEnteredEvent>.OnEvent += HandleBossCombatEntered;
            HandleBossCombatEntered(new BossCombatEnteredEvent(null));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus<BossCombatEnteredEvent>.OnEvent -= HandleBossCombatEntered;
        }

        private void HandleBossCombatEntered(BossCombatEnteredEvent evt)
        {
            Boss boss = evt.Boss;
            if (boss == null)
            {
                Hide();
                return;
            }

            drawingHealthCompo = boss.GetCompo<HealthCompo>();
            drawingHealthCompo.OnTakeDamage += HandleTakeDamage;
            SetBar(drawingHealthCompo.CurrentValue, drawingHealthCompo.MaxValue);

            if (boss.EnemyData == null)
                SetNameText("Umm : Unknown : ");
            else
                SetNameText(boss.EnemyData.name);

            Show();
        }

        private void HandleTakeDamage(float damage)
        {
            if (drawingHealthCompo.CurrentValue <= 0)
            {
                gameObject.SetActive(false);
                drawingHealthCompo.OnTakeDamage -= HandleTakeDamage;
            }

            SetBar(drawingHealthCompo.CurrentValue, drawingHealthCompo.MaxValue);
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