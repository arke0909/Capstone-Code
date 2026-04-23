using Chipmunk.ComponentContainers;
using Code.SHS.Entities.Enemies;
using Code.SHS.Entities.Enemies.FSM;
using Scripts.Combat;
using Scripts.Combat.Datas;
using Scripts.Entities;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Scripts.Enemies.EnemyBehaviours
{
    public class EnemyAttackBehaviour : EnemyBehaviour
    {
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
            Weapon weapon = _attackCompo.GetCurrentWeapon<Weapon>();
            if (weapon is not IAttackable attackable)
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