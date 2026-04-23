using System;
using System.Collections.Generic;
using Scripts.SkillSystem.Manage;
using Scripts.SkillSystem;
using UnityEngine;

namespace Work.Code.SkillInventory
{
    public class SkillEquipModel
    {
        private readonly Skill[] _activeSkills = new Skill[3];
        private readonly Skill[] _passiveSkills = new Skill[3];
        
        public event Action<Skill> OnSkillUnequipped;
        public event Action OnSkillChanged;

        public void Equip(Skill sendSkill, Skill targetSkill, int index, SkillType type, bool isInventory)
        {
            var skills = GetSkills(type);
            int sendIndex = Array.IndexOf(skills, sendSkill);

            if (isInventory)
            {
                if (targetSkill != null)
                    SwapDuplicate(targetSkill, sendSkill, skills);
                else
                    RemoveDuplicate(sendSkill, index, skills);

                skills[index] = sendSkill;
            }
            else
            {
                skills[sendIndex] = targetSkill;
                skills[index] = sendSkill;
            }
            
            OnSkillChanged?.Invoke();
        }
        
        private void SwapDuplicate(Skill target, Skill send, Skill[] skills)
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (skills[i] == send)
                {
                    skills[i] = target;
                    return;
                }
            }
        }

        public Skill GetSkill(int index, SkillType type)
        {
            var skills = GetSkills(type);
            if (index < 0 || index >= skills.Length) return null;
            return skills[index];
        }

        private void RemoveDuplicate(Skill skill, int target, Skill[] skills)
        {
            for (int i = 0; i < skills.Length; i++)
            {
                if (i == target) continue;
                if (skills[i] == skill)
                {
                    skills[target] = skill;
                    skills[i] = null;
                    return;
                }
            }
        }

        public void RemoveSkill(Skill target)
        {
            if (target == null) return;

            bool isChanged = false;

            for (int i = 0; i < _activeSkills.Length; i++)
            {
                if (_activeSkills[i] == target)
                {
                    _activeSkills[i] = null;
                    isChanged = true;
                    break;
                }
            }

            for (int i = 0; i < _passiveSkills.Length; i++)
            {
                if (_passiveSkills[i] == target)
                {
                    _passiveSkills[i] = null;
                    isChanged = true;
                    break;
                }
            }

            if (isChanged)
                OnSkillChanged?.Invoke();
        }

        private Skill[] GetSkills(SkillType type)
        {
            return type == SkillType.Active ? _activeSkills : _passiveSkills;
        }
    }
}