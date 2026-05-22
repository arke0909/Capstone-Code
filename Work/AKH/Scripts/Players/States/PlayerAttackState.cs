using Ami.BroAudio.Demo;
using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.ETC;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Code.Players;
using Scripts.Combat.Datas;
using SHS.Scripts.Entities.Rigings;
using UnityEngine;
using Code.Items;
using UnityEngine.Animations.Rigging;

namespace Scripts.Players.States
{
    public class PlayerAttackState : PlayerMoveState
    {
        private IAttackable _weaponItem;
        private IAimProvider _aimProvider;
        private PlayerEquipment _equipment;
        private CharacterMovement _movement;
        private RigBuilderController _rigBuilderController;

        public PlayerAttackState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Aim;
            _equipment = container.Get<PlayerEquipment>();
            _aimProvider = container.GetSubclassComponent<IAimProvider>();
            _movement = container.Get<CharacterMovement>();
            _rigBuilderController = container.Get<RigBuilderController>();
        }

        public override void Enter()
        {
            base.Enter();
            Vector3 aimDirection = _aimProvider.GetAimPosition() - _player.transform.position;
            aimDirection.y = 0;
            _movement.SetRotationInfo(aimDirection, 0);
            _movement.SetRotation(aimDirection);
            _animatorTrigger.OnDamageCastTrigger += HandleDamageCast;
            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) &&
                item is IAttackable attackable)
                _weaponItem = attackable;
            _weaponItem.EnterAttack();
        }

        public override void Update()
        {
            base.Update();
            if (_player.PlayerInput.AttackKey && _weaponItem.CanAttack())
                _player.ChangeState(PlayerStateEnum.Attack, true);
            else if (_isTriggerCall)
                _player.ChangeState(PlayerStateEnum.Aim);
        }

        private void HandleDamageCast()
        {
            _weaponItem.AttackTrigger();
        }

        public override void Exit()
        {
            base.Exit();
            _animatorTrigger.OnDamageCastTrigger -= HandleDamageCast;
            if (_weaponItem is GunItem gun)
            {
                EventBus.Raise(new AmmoUpdateEvent(gun.CurrentBulletCnt, gun.GunItemData.maxAmmoCapacity));
                //int totalAmmo = Mathf.Min(gun.GunItemData.maxBullet, gun.currentBulletItem.Stack);
            }
        }
    }
}