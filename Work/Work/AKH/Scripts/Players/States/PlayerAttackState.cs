using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Code.Players;
using Scripts.Combat.Datas;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Scripts.Players.States
{
    public class PlayerAttackState : PlayerMoveState
    {
        private IAttackable _weaponItem;
        private PlayerEquipment _equipment;
        public PlayerAttackState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Aim;
            _equipment = container.Get<PlayerEquipment>();
        }
        public override void Enter()
        {
            base.Enter();
            _animatorTrigger.OnDamageCastTrigger += HandleDamageCast;
            if (_equipment.TryGetEquippedItem(EquipPartType.Hand, out EquipableItem item) && item is IAttackable attackable)
                _weaponItem = attackable;
            _weaponItem.EnterAttack();
        }
        public override void Update()
        {
            base.Update();
            if(_player.PlayerInput.AttackKey && _weaponItem.CanAttack())
                _player.ChangeState(PlayerStateEnum.Attack,true);
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
