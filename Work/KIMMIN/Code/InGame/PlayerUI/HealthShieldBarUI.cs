using TMPro;
using UnityEngine;

namespace InGame.PlayerUI
{
    public class HealthShieldBarUI : MonoBehaviour
    {
        [SerializeField] private RectTransform shieldFill;
        [SerializeField] private TextMeshProUGUI amountText;

        public void SetBar(float currentHealth, float maxHealth, float currentShield)
        {
            float healthRatio = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentHealth / maxHealth);
            float shieldRatio = maxHealth <= 0f ? 0f : Mathf.Clamp01(currentShield / maxHealth);
            float shieldEnd = Mathf.Clamp01(healthRatio + shieldRatio);
            float shieldStart = Mathf.Max(0f, shieldEnd - shieldRatio);

            shieldFill.gameObject.SetActive(currentShield > 0f);
            shieldFill.anchorMin = new Vector2(shieldStart, 0f);
            shieldFill.anchorMax = new Vector2(shieldEnd, 1f);
            shieldFill.offsetMin = Vector2.zero;
            shieldFill.offsetMax = Vector2.zero;

            int roundedHealth = Mathf.RoundToInt(currentHealth);
            int roundedMaxHealth = Mathf.RoundToInt(maxHealth);
            int roundedShield = Mathf.RoundToInt(currentShield);
            amountText.text = currentShield > 0f
                ? $"{roundedHealth} (+{roundedShield}) / {roundedMaxHealth}"
                : $"{roundedHealth} / {roundedMaxHealth}";
        }
    }
}
