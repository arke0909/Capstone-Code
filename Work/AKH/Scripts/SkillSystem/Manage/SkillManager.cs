using System;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.SkillSystem;
using Scripts.SkillSystem;
using System.Collections.Generic;
using System.Linq;
using Code.Items;
using UnityEngine;
using Work.Code.SkillInventory.GameEvents;

namespace Scripts.SkillSystem.Manage
{
    public enum SkillType
    {
        None,
        Passive,
        Active
    }

    public class SkillManager : MonoBehaviour, IContainerComponent, IAfterInitialze
    {
        public ComponentContainer ComponentContainer { get; set; }

        public Dictionary<SkillType, ISkillCompo> skillCompos;
        
        private Dictionary<SkillDataSO, HashSet<EquipableItem>> _skillOwners = new();
        private Dictionary<SkillDataSO, Skill> _skills = new();
        private LocalEventBus _localEventBus;

        public Action OnSkillEquip;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            skillCompos = GetComponentsInChildren<ISkillCompo>().ToDictionary(compo => compo.SkillType, compo => compo);
            _localEventBus = componentContainer.Get<LocalEventBus>();
            _localEventBus.Subscribe<EquipSkillEvent>(HandleEquipSkill);
            _localEventBus.Subscribe<UnEquipSkillEvnt>(HandleUnequipSkill);
        }

        public void AfterInitialize()
        {
            SetSkills(GetComponentsInChildren<Skill>(true));
        }

        private void HandleUnequipSkill(UnEquipSkillEvnt evt)
        {
            RemoveSkill(evt.UnEquippedSkill.SkillData);
        }

        private void OnDestroy()
        {
            _localEventBus.Unsubscribe<EquipSkillEvent>(HandleEquipSkill);
            _localEventBus.Unsubscribe<UnEquipSkillEvnt>(HandleUnequipSkill);
        }

        private void HandleEquipSkill(EquipSkillEvent @event)
        {
            ChangeSkill(@event.EquippedSkill.SkillData, @event.Index);
        }
        public void ChangeSkill(SkillDataSO skillData, int slotIndex)
        {
            if (TryGetSkill(skillData, out Skill skill))
            {
                SkillType skillEnumType = skill.SkillType;
                if (skillCompos.TryGetValue(skillEnumType, out ISkillCompo skillCompo))
                {
                    skillCompo.ChangeSkill(skillData, slotIndex);
                    OnSkillEquip?.Invoke();
                }
            }
        }
        
        public void RegisterEquippedSkill(EquipableItem item)
        {
            if (item == null || item.Skill == null)
                return;

            if (_skillOwners.TryGetValue(item.Skill, out var owners) == false)
            {
                owners = new HashSet<EquipableItem>();
                _skillOwners.Add(item.Skill, owners);
                AddSkill(item.Skill);
            }

            owners.Add(item);
            RefreshSkillLevel(item.Skill, owners);
        }

        public void DeregisterEquippedSkill(EquipableItem item)
        {
            if (item == null || item.Skill == null)
                return;

            if (_skillOwners.TryGetValue(item.Skill, out var owners) == false)
                return;

            owners.Remove(item);

            if (owners.Count == 0)
            {
                _skillOwners.Remove(item.Skill);
                RemoveSkill(item.Skill);
                return;
            }

            RefreshSkillLevel(item.Skill, owners);
        }

        private void RefreshSkillLevel(SkillDataSO skillData, HashSet<EquipableItem> owners)
        {
            if (TryGetSkill(skillData, out Skill skill) == false)
                return;

            int level = owners.Max(item => item.SkillLevel);
            skill.SetLevel(level);
        }

        public void AddSkill(SkillDataSO skillData)
        {
            if (skillData == null)
                return;
            
            if (TryGetSkill(skillData, out Skill skill))
            {
                SkillType skillEnumType = skill.SkillType;
                if (skillCompos.TryGetValue(skillEnumType, out ISkillCompo skillCompo))
                {
                    skillCompo.AddSkill(skill);
                }
            }
        }

        public void RemoveSkill(SkillDataSO skillData)
        {
            if (skillData == null)
                return;
            if (TryGetSkill(skillData, out Skill skill))
            {
                SkillType skillEnumType = skill.SkillType;
                if (skillCompos.TryGetValue(skillEnumType, out ISkillCompo skillCompo))
                {
                    skillCompo.RemoveSkill(skill);
                }
            }
        }

        public void SetSkills(IEnumerable<Skill> skills)
        {
            _skills.Clear();
            foreach (var item in skills)
            {
                if (item.SkillData == null)
                {
                    Debug.LogError($"Skill {item.name} has null SkillData", item);
                    continue;
                }

                _skills[item.SkillData] = item;
                item.Init(ComponentContainer);
            }
        }

        public bool TryGetSkill(SkillDataSO skillData, out Skill skill)
        {
            skill = null;
            if (skillData == null)
                return false;
            return _skills.TryGetValue(skillData, out skill);
        }
    }
}