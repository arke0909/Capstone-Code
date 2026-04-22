using System;
using System.Linq;
using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Scripts.Players;
using Scripts.SkillSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Work.Code.SkillInventory
{
    public class SkillInventoryUI : MonoBehaviour
    {
        private SkillSlot[] _skillUIs;

        private ActiveSkillComponent _activeSkillCompo;
        private PassiveSkillComponent _passiveSkillCompo;
        public event Action<Skill[]> OnChangeInventory;

        public void Initialize(Player player)
        {
            _skillUIs = GetComponentsInChildren<SkillSlot>();

            _activeSkillCompo = player.Get<ActiveSkillComponent>();
            _passiveSkillCompo = player.Get<PassiveSkillComponent>();

            _activeSkillCompo.OnSkillsChanged += UpdateSkills;
            _passiveSkillCompo.OnSkillsChanged += UpdateSkills;

            foreach (var ui in _skillUIs)
            {
                ui.IsInventorySlot = true;
            }

            UpdateSkills();
        }

        private void Update()
        {
            if(Keyboard.current.lKey.wasPressedThisFrame)
                UpdateSkills();
        }

        private void OnDestroy()
        {
            _activeSkillCompo.OnSkillsChanged -= UpdateSkills;
            _passiveSkillCompo.OnSkillsChanged -= UpdateSkills;
        }

        public void UpdateSkills()
        {
            if (_activeSkillCompo == null || _passiveSkillCompo == null) return;
            
            var activeSkills = _activeSkillCompo.Skills.Values.ToList();
            var passiveSkills = _passiveSkillCompo.Skills.Values.ToList();
            var skills = activeSkills.Union(passiveSkills)
                .Where(skill => !skill.SkillData.defaultSkill).ToArray();
            
            UpdateInventory(skills);
        }

        private void UpdateInventory(Skill[] skills)
        {
            for (int i = 0; i < _skillUIs.Length; i++)
            {
                if (i < skills.Length)
                    _skillUIs[i].EnableFor(skills[i]);
                else
                    _skillUIs[i].Clear();
            }
            
            OnChangeInventory?.Invoke(skills);
        }

        public void RemoveSkill(Skill skill)
        {
            foreach (var skillUI in _skillUIs)
            {
                if (skillUI.CurrentSkill.SkillData == skill.SkillData)
                {
                    skillUI.DisableUI();
                    return;
                }
            }
        }
    }
}