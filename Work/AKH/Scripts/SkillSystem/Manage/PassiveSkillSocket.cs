using Scripts.SkillSystem;

namespace Scripts.SkillSystem.Manage
{
    public class PassiveSkillSocket : SkillSocket
    {
        public PassiveSkill CurrentPassiveSkill { get; private set; }

        public override void ChangeItem(Skill newSkill)
        {
            if (newSkill != null && newSkill is not PassiveSkill)
            {
                UnityEngine.Debug.LogWarning("Invalid SkillType");
                return;
            }

            if (ReferenceEquals(CurrentPassiveSkill, newSkill))
            {
                base.ChangeItem(newSkill);
                return;
            }

            CurrentPassiveSkill?.DisableSkill();
            CurrentPassiveSkill = newSkill as PassiveSkill;
            base.ChangeItem(newSkill);
            CurrentPassiveSkill?.EnableSkill();
        }
    }
}
