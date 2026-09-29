using Scripts.SkillSystem.Manage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI.Core.Interaction;

namespace Scripts.SkillSystem.UI
{
    public class PassiveSkillUI : InteractableUI
    {
        [SerializeField] private TextMeshProUGUI skillName;
        [SerializeField] private Image skillIcon;

        private PassiveSkillSocket _socket;
        private SkillSlotAttentionUI _attention;

        protected override void Awake()
        {
            base.Awake();
            _attention = new SkillSlotAttentionUI(transform);
            DisableUI();
        }

        protected override void OnDestroy()
        {
            UnsubscribeCurrentSkill();
            _attention?.Dispose();
            base.OnDestroy();
        }

        public void Init(PassiveSkillSocket socket)
        {
            _attention.StopAttention();

            if (socket == null)
            {
                DisableUI();
                return;
            }

            EnableUI();
            _socket = socket;

            skillIcon.gameObject.SetActive(true);
            skillName.text = socket.CurrentPassiveSkill.SkillData.skillName;
            skillIcon.sprite = socket.CurrentPassiveSkill.SkillData.skillIcon;

            socket.CurrentPassiveSkill.OnSkillInvoked += HandleSkillInvoked;
        }

        public void ShowEmptyAttention()
        {
            if (_socket != null)
                return;

            EnableUI();
            skillIcon.gameObject.SetActive(false);
            skillName.text = "패시브 슬롯";
            _attention.PlayAttention();
        }

        public void StopAttention()
        {
            _attention.StopAttention();

            if (_socket == null)
                DisableUI();
        }

        public override void DisableUI(bool isFade = false)
        {
            _attention.StopAttention();
            skillIcon.gameObject.SetActive(false);
            skillName.text = string.Empty;
            base.DisableUI(isFade);
            UnsubscribeCurrentSkill();
        }

        private void UnsubscribeCurrentSkill()
        {
            if (_socket == null || _socket.CurrentPassiveSkill == null)
                return;

            _socket.CurrentPassiveSkill.OnSkillInvoked -= HandleSkillInvoked;
            _socket = null;
        }

        private void HandleSkillInvoked()
        {
            //나중에 이펙트 추가
        }
    }
}
