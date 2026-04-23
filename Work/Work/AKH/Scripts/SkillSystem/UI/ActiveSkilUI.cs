using Code.SkillSystem;
using Scripts.SkillSystem.Manage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Scripts.SkillSystem.UI
{
    public class ActiveSkilUI : InteractableUI
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI cooldownText;

        private ActiveSkillSocket _activeSocket;

        public void InitSlot(ActiveSkillSocket socket, ActiveSlotType slotType)
        {
            if (socket == null)
            {
                DisableUI();
                return;
            }
            
            _activeSocket = socket;
            icon.gameObject.SetActive(true);
            icon.sprite = socket.CurrentSkill.SkillData.skillIcon;
            socket.OnCoolDown += HandleCooldown;
        }

        private void HandleCooldown(SkillDataSO skilldata, float current, float total)
        {
            fill.fillAmount = current / total;
            cooldownText.text = current <= 0f ? string.Empty : $"{current:F1}s";
        }

        public override void DisableUI(bool hasTween = false)
        {
            icon.gameObject.SetActive(false);
            fill.fillAmount = 0f;
            cooldownText.text = string.Empty;
            
            if (_activeSocket != null)
            {
                _activeSocket.OnCoolDown -= HandleCooldown;
                _activeSocket = null;
            }
        }
    }
}
