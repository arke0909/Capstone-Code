using Scripts.FSM;
using Scripts.SkillSystem.Manage;
using Scripts.SkillSystem.Skills;
using UnityEngine;

namespace Scripts.SkillSystem
{
    public abstract class ActiveSkill : Skill, IUseStateSkill
    {
        public bool isWalkable;
        public float cooldown;
        
        [field: SerializeField] public SkillAnimType AnimType { get; set; }
        [field: SerializeField] public SkillMoveType MoveType { get; set; } = SkillMoveType.Move;
        [field: SerializeField] public StateDataSO TargetState { get; set; }
        
        public virtual bool CanUseSkill()
            => true;
        public sealed override SkillType SkillType
            => SkillType.Active;

        public virtual void StartSkill()
        {
        }

        public virtual void EndSkill()
        {
        }
        public virtual void OnSkillTrigger()
        {
        }

    }
}
