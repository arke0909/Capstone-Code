namespace Scripts.SkillSystem.Skills
{
    public interface IAimSkill : IUseStateSkill
    {
        public void StartAiming();
        public void CancelSkill();
    }
}
