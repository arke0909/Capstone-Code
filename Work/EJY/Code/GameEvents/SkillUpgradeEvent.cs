using Chipmunk.GameEvents;
using Code.SkillSystem.Upgrade;
using Scripts.SkillSystem;

namespace Code.GameEvents
{
    public struct SkillUpgradeEvent : IEvent
    {
        public Skill targetSkill;
        public SkillUpgradeSO upgradeData;

        public SkillUpgradeEvent(Skill skill, SkillUpgradeSO data)
        {
            this.targetSkill = skill;
            this.upgradeData = data;
        }
    }
}