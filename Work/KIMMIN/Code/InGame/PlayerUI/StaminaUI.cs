using Code.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.PlayerUI
{
    public class StaminaUI : UIBase
    {
        [SerializeField] private Image fill;
        private readonly Color _endColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        
        public void SetFill(float amount)
        {
            fill.fillAmount = amount;
            float t = Mathf.InverseLerp(0.15f, 0.65f, amount);
            fill.color = Color.Lerp(_endColor, Color.white, t);
        }
    }
}