using System;
using Code.SkillSystem;
using DG.Tweening;
using Scripts.SkillSystem.Manage;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Scripts.SkillSystem.UI
{
    public class ActiveSkilUI : InteractableUI
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI keyText;
        [SerializeField] private PlayerInputSO playerInput;

        private ActiveSkillSocket _activeSocket;
        private ActiveSlotType _slotType;
        private SkillSlotAttentionUI _attention;

        protected override void Awake()
        {
            base.Awake();
            _attention = new SkillSlotAttentionUI(transform);
            playerInput.OnKeyMapped += HandleKeyMapped;
        }

        private void OnEnable()
        {
            RefreshKeyText();
        }

        protected override void OnDestroy()
        {
            _attention?.Dispose();
            playerInput.OnKeyMapped -= HandleKeyMapped;
            base.OnDestroy();
        }

        public void InitSlot(ActiveSkillSocket socket, ActiveSlotType slotType)
        {
            _attention.StopAttention();
            _slotType = slotType;
            RefreshKeyText();

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

        public void ShowEmptyAttention()
        {
            if (_activeSocket == null)
                _attention.PlayAttention();
        }

        public void StopAttention()
        {
            _attention.StopAttention();
        }

        private void HandleCooldown(SkillDataSO skilldata, float current, float total)
        {
            fill.fillAmount = current / total;
            cooldownText.text = current <= 0f ? string.Empty : $"{current:F1}s";
        }

        public override void DisableUI(bool hasTween = false)
        {
            _attention.StopAttention();
            icon.gameObject.SetActive(false);
            fill.fillAmount = 0f;
            cooldownText.text = string.Empty;

            if (_activeSocket != null)
            {
                _activeSocket.OnCoolDown -= HandleCooldown;
                _activeSocket = null;
            }
        }

        private void HandleKeyMapped(Key targetKey, Key newKey)
        {
            RefreshKeyText();
        }

        private void RefreshKeyText()
        {
            keyText.text = playerInput.GetKeyText(GetKey(_slotType));
        }

        private Key GetKey(ActiveSlotType slotType)
        {
            return slotType switch
            {
                ActiveSlotType.Q => Key.Q,
                ActiveSlotType.E => Key.E,
                ActiveSlotType.C => Key.C,
                ActiveSlotType.Space => Key.Space,
                _ => Key.None
            };
        }
    }
}
