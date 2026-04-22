using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Scripts.Combat.Datas;
using Scripts.Entities;
using Scripts.SkillSystem;
using UnityEngine;

namespace Code.SHS.Entities.Enemies.Skills
{
    /// <summary>
    /// Enemy 전용 DashSkill - CharacterNavMovement를 사용
    /// </summary>
    public class EnemyDashSkill : ActiveSkill
    {
        [SerializeField] private MovementDataSO movementData;
        private CharacterNavMovement _movement;
        private Enemy _enemy;

        public override void Init(ComponentContainer container)
        {
            base.Init(container);
            _movement = container.Get<CharacterNavMovement>();
            _enemy = container.Get<Enemy>();
        }

        public override void StartAndUseSkill()
        {
            // 네비게이션 멈추고 대시 실행
            _movement.SetStop(true);
            
            // 타겟(플레이어) 방향으로 대시
            Vector3 direction = GetDashDirection();
            _movement.LookAtTarget(_enemy.transform.position + direction, false);
            _movement.KnockBack(direction, movementData);
        }

        public override void EndSkill()
        {
            _movement.SetStop(false);
        }

        /// <summary>
        /// 대시 방향 계산 - 타겟이 있으면 타겟 방향, 없으면 현재 바라보는 방향
        /// </summary>
        private Vector3 GetDashDirection()
        {
            Entity target = _enemy.TargetProvider.CurrentTarget;
            if (target != null)
            {
                Vector3 direction = (target.transform.position - _enemy.transform.position).normalized;
                direction.y = 0;
                return direction;
            }
            return _enemy.transform.forward;
        }
    }
}


