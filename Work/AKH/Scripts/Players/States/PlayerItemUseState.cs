using Chipmunk.ComponentContainers;
using Code.InventorySystems.Equipments;
using Code.Players;
using UnityEngine;
using Code.Items;

namespace Scripts.Players.States
{
    public class ItemUseContext
    {
        public UsableItem TargetItem { get; set; }
    }
    public class PlayerItemUseState : PlayerMoveState
    {
        private UsableItem _item;
        private PlayerEquipment _equipment;
        private PlayerInventory _inventory;
        public PlayerItemUseState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Walk;
            _equipment = container.Get<PlayerEquipment>();
            _inventory = container.Get<PlayerInventory>();
        }
        public override void Enter()
        {
            base.Enter();
            ItemUseContext context = _blackboard.GetOrDefault<ItemUseContext>("ItemUseContext");
            Debug.Assert(context != null, "Context is null");
            _item = context.TargetItem;
        }
        public override void Update()
        {
            base.Update();
            if (_isTriggerCall) 
                _player.ChangeState(PlayerStateEnum.Idle);
        }
        public override void Exit()
        {
            base.Exit();

            if (_item != null && _inventory.RemoveItem(_item, 1, false))
            {
                _item.Use(_player);
            }

            _equipment.RestoreHandledEquip();
        }
    }
}

