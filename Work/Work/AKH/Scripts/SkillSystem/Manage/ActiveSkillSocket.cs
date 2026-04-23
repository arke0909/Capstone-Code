using Code.SkillSystem;
using System;
using UnityEngine;

namespace Scripts.SkillSystem.Manage
{
    public delegate void OnCoolDown(SkillDataSO skillData, float current, float total);
    [Serializable]
    public class ActiveSkillSocket : SkillSocket
    {
        public event OnCoolDown OnCoolDown;
        private float _cooldownTimer;
        public ActiveSkill CurrentActiveSkill { get; set; }

        public void UpdateSocket()
        {
            if (_cooldownTimer >= 0 && CurrentActiveSkill != null)
            {
                _cooldownTimer -= Time.deltaTime;
                if (_cooldownTimer <= 0)
                    _cooldownTimer = 0;

                OnCoolDown?.Invoke(CurrentActiveSkill.SkillData, _cooldownTimer, CurrentActiveSkill.cooldown);
            }
        }
        public bool CanUseSkill()
            => _cooldownTimer <= 0f && CurrentActiveSkill != null && CurrentActiveSkill.CanUseSkill();
        public void SetCooldown()
            => _cooldownTimer = CurrentActiveSkill.cooldown;

        public override void ChangeItem(Skill newSkill)
        {
            if (newSkill is not ActiveSkill active)
            {
                if (newSkill == null)
                {
                    CurrentActiveSkill = null;
                    base.ChangeItem(newSkill);
                }
                else
                {
                    Debug.LogWarning("Invalid SkillType");
                }
                return;
            }
            CurrentActiveSkill = active;
            base.ChangeItem(newSkill);
        }
    }
}

