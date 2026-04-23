using Scripts.SkillSystem.Manage;
using Chipmunk.GameEvents;
using Code.SkillSystem;

namespace Scripts.SkillSystem.SkillEvents
{

    public struct SkillCooldownEvent : IEvent
    {
        public ActiveSlotType slotType;
        public SkillDataSO skillData;
        public float current;
        public float total;
    }
}
