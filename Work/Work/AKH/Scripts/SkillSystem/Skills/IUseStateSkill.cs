using Scripts.FSM;

namespace Scripts.SkillSystem.Skills
{
    public enum SkillAnimType
    {
        Default = 0,
        Rolling = 1,
        Grab = 2,
    }
    public interface IUseStateSkill
    {
        SkillAnimType AnimType { get; }
        StateDataSO TargetState { get; }
        void OnSkillTrigger();
    }
}

