using Scripts.FSM;

namespace Scripts.SkillSystem.Skills
{
    public enum SkillAnimType
    {
        Default = 0,
        Rolling = 1,
        Grab = 2,
        Fire = 3,
        Burrow = 4,
        PickaxeSlam = 5,
        CaveIn = 6,
        Smash = 7,
    }

    public enum SkillMoveType
    {
        Stop = 0, // 움직이면서 사용 불가능
        Move = 1, // 움직이면서 사용 가능
        Force = 2, // 스킬 자체가 이동
    }

    public interface IUseStateSkill
    {
        SkillAnimType AnimType { get; }
        StateDataSO TargetState { get; }
        void OnSkillTrigger();
    }
}
