using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Code.SHS.Entities.Enemies.FSM.BehaviourState;
using Scripts.Combat;
using Scripts.Combat.Datas;
using UnityEngine;
using Code.Items;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemyAttackBehaviour : EnemyBehaviour
    {
        [SerializeField] private int attackCount;
        private AttackCompo _attackCompo;
        private EnemyInventory _inventory;

        public override void Init(Enemy enemy)
        {
            base.Init(enemy);
            _inventory = enemy.Get<EnemyInventory>();
            _attackCompo = enemy.Get<AttackCompo>();
        }

        public override void Execute()
        {
            IAttackable attackable = _attackCompo.CurrentAttackable;
            if(_enemy.Blackboard.TryGet<EnemyAttackContext>("AttackContext", out var context))
            {
                context.attackCount = attackCount;
            }
            else
            {
                EnemyAttackContext newContext = new EnemyAttackContext() { attackCount = attackCount };
                _enemy.Blackboard.Set("AttackContext", newContext);
            }
            if (attackable == null)
                return;
            switch (attackable.CurrentAttackableState)
            {
                case AttackableState.CanAttack:
                    _enemy.ChangeState(EnemyStateEnum.Attack, true);
                    break;
                case AttackableState.NeedAmmo:
                    if (attackable is IReloadable)
                        _enemy.ChangeState(EnemyStateEnum.Reload);
                    break;
                case AttackableState.NeedStack:
                    if (attackable is Weapon weapon)
                        _inventory.TryAddItem(weapon);
                    break;
                case AttackableState.NotEquipped:
                    break;
                case AttackableState.Delayed:
                    break;
            }
        }
    }
}
