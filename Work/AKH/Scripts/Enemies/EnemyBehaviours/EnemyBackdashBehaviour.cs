using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Scripts.Combat;
using Scripts.Combat.Datas;
using UnityEngine;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemyBackdashBehaviour : EnemyBehaviour
    {
        [SerializeField] private MovementDataSO dashMovement;
        private CharacterNavMovement _movement;
        private AttackCompo _attackCompo;
        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _attackCompo = enemy.Get<AttackCompo>();
            _movement = enemy.Get<CharacterNavMovement>();
        }

        public override void Execute()
        {
            Vector3 direction = _enemy.transform.position - _enemy.TargetProvider.LastTargetPosition;
            _movement.KnockBack(direction.normalized,dashMovement);
            SetCooldown();
        }
    }
}
