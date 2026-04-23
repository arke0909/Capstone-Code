using System;
using System.Collections.Generic;
using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Chipmunk.Library.Utility.GameEvents.Local;
using Code.EnemySpawn;
using Code.SHS.Entities.Enemies.Events.Local;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Skills
{
    public class EnemySkillSpawner : MonoBehaviour, IContainerComponent, ILocalEventSubscriber<EnemySpawnEvent>
    {
        public ComponentContainer ComponentContainer { get; set; }
        private SkillManager _skillManager;
        private ActiveSkillComponent _activeSkillComponent;
        private PassiveSkillComponent _passiveSkillComponent;

        public void OnInitialize(ComponentContainer componentContainer)
        {
            _skillManager = componentContainer.Get<SkillManager>();
            _activeSkillComponent = componentContainer.Get<ActiveSkillComponent>();
            _passiveSkillComponent = componentContainer.Get<PassiveSkillComponent>();
        }

        public void OnLocalEvent(EnemySpawnEvent eventData)
        {
            EnemySO enemySO = eventData.EnemyData;
            SpawnSkills(enemySO);
            EquipSkills(enemySO);
        }


        private void SpawnSkills(EnemySO enemySO)
        {
            List<Skill> skillInstances = new(6);
            foreach (var skillPatch in enemySO.passiveSkill.Values)
            {
                if (skillPatch == null)
                    continue;
                PassiveSkill skill = Instantiate(skillPatch.Value, _passiveSkillComponent.transform);
                skill.transform.position = transform.position;
                skillPatch.ApplySetter(skill);
                skillInstances.Add(skill);
            }


            foreach (var skillPatch in enemySO.activeSkill.Values)
            {
                if (skillPatch == null)
                    continue;
                ActiveSkill skill = Instantiate(skillPatch.Value, _activeSkillComponent.transform);
                skill.transform.position = transform.position;
                skillPatch.ApplySetter(skill);
                skillInstances.Add(skill);
            }

            _skillManager.SetSkills(skillInstances);
            foreach (var skill in skillInstances)
                _skillManager.AddSkill(skill.SkillData);
        }

        private void EquipSkills(EnemySO enemySO)
        {
            foreach(var patch in enemySO.activeSkill)
            {
                if (patch.Value == null)
                    continue;
                ActiveSkill activeSkill = patch.Value;
                _activeSkillComponent.ChangeSkill(activeSkill.SkillData, patch.Key);
            }

            foreach(var patch in enemySO.passiveSkill)
            {
                if (patch.Value == null)
                    continue;
                PassiveSkill passiveSkill = patch.Value;
                _passiveSkillComponent.ChangeSkill(passiveSkill.SkillData, patch.Key);
            }
        }
    }
}