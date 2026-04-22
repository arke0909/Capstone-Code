using Code.SkillSystem;
using Scripts.SkillSystem;
using System;
using System.Collections.Generic;

namespace Scripts.SkillSystem.Manage
{
    public interface ISkillCompo
    {
        SkillType SkillType { get; }
        Dictionary<SkillDataSO, Skill> Skills { get; }
        Skill GetSkill(SkillDataSO skillData);
        void ChangeSkill(SkillDataSO skillData, int slotType);
        void AddSkill(Skill skill);
        void RemoveSkill(Skill skill);
    }
}
