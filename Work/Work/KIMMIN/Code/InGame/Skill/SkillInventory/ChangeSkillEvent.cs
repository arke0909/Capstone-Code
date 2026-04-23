using System.Collections.Generic;
using Chipmunk.GameEvents;
using Scripts.SkillSystem;

namespace Work.Code.SkillInventory
{
    public struct ChangeSkillEvent : IEvent
    {
        public List<Skill> activeSkills;
    }
}