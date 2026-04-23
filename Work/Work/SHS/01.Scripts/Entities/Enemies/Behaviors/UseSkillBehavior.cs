using System;
using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Scripts.Enemies.EnemyBehaviours;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Behaviors
{
    public class UseSkillBehavior : EnemyBehaviour
    {
        [SerializeField] private ActiveSlotType slotType;

        private ActiveSkillComponent _skillComponent;

        public ComponentContainer ComponentContainer { get; set; }
        public ActiveSlotType SlotType => slotType;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _skillComponent = enemy.ComponentContainer.Get<ActiveSkillComponent>(true);
        }

        public override void Execute()
        {
            _skillComponent.CurrentSkillIndex = slotType;
            _skillComponent.UseSkill();
            Debug.Log(
                $"<color=red>Use Skill : </color> <color=yellow>{_skillComponent.CurrentSkillIndex} </color>{_skillComponent.CurrentSkill?.name}",
                this);
            SetCooldown();
        }
    }
}