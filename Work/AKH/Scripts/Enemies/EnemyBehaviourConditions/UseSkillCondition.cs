using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Scripts.SkillSystem.Manage;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviourConditions
{
    public class UseSkillCondition : EnemyBehaviourCondition
    {
        [SerializeField] private ActiveSlotType slotType;

        private ActiveSkillComponent _skillComponent;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _skillComponent = enemy.Get<ActiveSkillComponent>();
        }
        public override bool Condition()
        {
            if (_enemy == null || _skillComponent == null)
                return false;

            if (_skillComponent.Sockets.TryGetValue(slotType, out ActiveSkillSocket skillSocket))
                return skillSocket.CanUseSkill();
            else
                return false;
        }
    }
}
