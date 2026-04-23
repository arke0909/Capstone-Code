using System;
using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Scripts.Entities;
using UnityEngine;

namespace Scripts.SkillSystem
{
    public abstract class PassiveSkill : Skill
    {
        public Action OnSkillInvoked;
        public bool Enabled { get; private set; } = false;
        
        
        public sealed override SkillType SkillType => SkillType.Passive;
        public virtual void EnableSkill()
        {
            Enabled = true;
        }
        public virtual void DisableSkill()
        {
            Enabled = false;
        }
    }
}

