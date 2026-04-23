using Scripts.SkillSystem.Manage;
using UnityEngine;

namespace Scripts.SkillSystem
{
    public abstract class ActiveSkill : Skill
    {
        public float cooldown;

        public virtual bool CanUseSkill()
            => true;
        public sealed override SkillType SkillType
            => SkillType.Active;

        public virtual void StartAndUseSkill()
        {
        }

        public virtual void EndSkill()
        {
        }
    }
}
