using Scripts.SkillSystem.Manage;
using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Code.Players;
using Scripts.Combat.Datas;
using Code.Items;
using SHS.Scripts.Combats;
using UnityEngine;

namespace Scripts.Players.States
{
    public abstract class PlayerCombatState : PlayerMoveState
    {
        protected Weapon _weapon;
        protected IAttackable _attackable;
        protected ActiveSkillComponent _skillCompo;
        protected PlayerEquipment _equipment;
        protected DefaultAttack _defaultAttack;

        protected PlayerCombatState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _skillCompo = container.Get<ActiveSkillComponent>(true);
            _equipment = container.Get<PlayerEquipment>();
            _defaultAttack = container.Get<DefaultAttack>();
            Debug.Assert(_defaultAttack != null, "Player requires DefaultAttack for unarmed attacks.");
        }
        public override void Enter()
        {
            base.Enter();
            _player.PlayerInput.OnSkillPressed += HandleSkillPressed;
            _player.PlayerInput.OnReloadPressed += HandleReloadPressed;
            _weapon = null;
            _attackable = null;

            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item))
            {
                if (item is Weapon weapon)
                    _weapon = weapon;

                if (item is IAttackable attackable)
                    _attackable = attackable;
            }
            else
            {
                _attackable = _defaultAttack;
            }
        }

        public override void Update()
        {
            base.Update();
            _attackable?.UpdateAttack(new AttackContext(false, false, _player.PlayerInput.AimKey));
        }

        public override void Exit()
        {
            base.Exit();
            _player.PlayerInput.OnSkillPressed -= HandleSkillPressed;
            _player.PlayerInput.OnReloadPressed -= HandleReloadPressed;
        }
        private void HandleSkillPressed(ActiveSlotType obj)
        {
            _skillCompo.CurrentSkillIndex = obj;
            if (_skillCompo.CanUseSkill())
                _skillCompo.UseSkill();
        }
        private void HandleReloadPressed()
        {
            //장전 가능한지 (총장착중인지, 총알이 남아있는지) 판별
            if (_weapon != null && _weapon is IReloadable reloadable && reloadable.CanReload)
            {
                _player.ChangeState(PlayerStateEnum.Reload);
            }
        }
    }
}
