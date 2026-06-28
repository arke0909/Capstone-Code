using Scripts.Combat.Datas;

namespace Scripts.SkillSystem.Skills
{
    public interface IMovementDataProvider
    {
        MovementDataSO MovementData { get; }
    }
}
