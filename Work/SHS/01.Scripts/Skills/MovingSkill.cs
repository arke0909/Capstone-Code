using Scripts.SkillSystem;
using UnityEngine;

namespace Scripts.SkillSystem.Skills
{
    public abstract class MovingSkill : ActiveSkill
    {
        [SerializeField] private float duration = 2f;

        public float Duration => duration;
    }
}
