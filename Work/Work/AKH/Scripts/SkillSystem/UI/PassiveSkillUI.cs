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

        protected override void Awake()
        {
            base.Awake();
            DisableUI();
        }

        public void Init(PassiveSkillSocket socket)
        {
            if (socket == null) { 
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

        public override void DisableUI(bool isFade = false)
        {
            base.DisableUI(isFade);
            if (_socket != null && _socket.CurrentPassiveSkill != null)
            {
                _socket.CurrentPassiveSkill.OnSkillInvoked += HandleSkillInvoked;
                _socket = null;
            }
        }

        private void HandleSkillInvoked()
        {
            //나중에 이펙트 추가
        }
    }
}