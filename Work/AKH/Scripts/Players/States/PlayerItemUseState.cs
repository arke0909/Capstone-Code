using Chipmunk.ComponentContainers;
using Code.InventorySystems;
using Chipmunk.GameEvents;
using Code.Players;
using UnityEngine;
using Code.Items;
using Code.Items.ItemInfo;
using SHS.Scripts.Combats.Events;
using Work.Code.GameEvents;

namespace Scripts.Players.States
{
    public class ItemUseContext
    {
        public UsableItem TargetItem { get; set; }
        public bool ShouldRestoreHandledItem { get; set; }
    }
    public class PlayerItemUseState : PlayerMoveState
    {
        private const string UseGageText = "사용 중";

        private UsableItem _item;
        private HandlingComponent _handlingComponent;
        private PlayerInventory _inventory;
        private bool _shouldRestoreHandledItem;
        private bool _isUseCompleted;
        private float _elapsedTime;
        private float _useDuration;

        public PlayerItemUseState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _myMoveType = MoveType.Walk;
            _handlingComponent = container.Get<HandlingComponent>();
            _inventory = container.Get<PlayerInventory>();
        }
        public override void Enter()
        {
            base.Enter();

            _isUseCompleted = false;
            _elapsedTime = 0f;

            ItemUseContext context = _blackboard.GetOrDefault<ItemUseContext>("ItemUseContext");
            if (context?.TargetItem?.ItemData is not UseItemDataSO useItemData)
            {
                Debug.LogError("Item use context or use item data is invalid.");
                _player.ChangeState(PlayerStateEnum.Idle);
                return;
            }

            _item = context.TargetItem;
            _shouldRestoreHandledItem = context.ShouldRestoreHandledItem;
            _useDuration = useItemData.useDuration;

            if (_useDuration <= 0f)
            {
                Debug.LogError($"Use duration must be greater than zero: {_item.ItemData.name}");
                _player.ChangeState(PlayerStateEnum.Idle);
                return;
            }

            _player.LocalEventBus.Subscribe<EntityDeadEvent>(HandleEntityDead);
            EventBus.Raise(new PlayerGageEvent(UseGageText, _useDuration, null));
        }

        public override void Update()
        {
            if (_player.IsDead)
                return;

            base.Update();

            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < _useDuration)
                return;

            _isUseCompleted = true;
            _player.ChangeState(PlayerStateEnum.Idle);
        }
        public override void Exit()
        {
            base.Exit();
            _player.LocalEventBus.Unsubscribe<EntityDeadEvent>(HandleEntityDead);
            EventBus.Raise(new StopPlayerGageEvent());

            if (_isUseCompleted && !_player.IsDead && _item != null && _inventory.RemoveItem(_item, 1, false))
            {
                _item.Use(_player);
            }

            if (_shouldRestoreHandledItem)
                _handlingComponent.RestoreHandledEquip();

            _item = null;
            _shouldRestoreHandledItem = false;
            _isUseCompleted = false;
            _elapsedTime = 0f;
            _useDuration = 0f;
        }

        private void HandleEntityDead(EntityDeadEvent evt)
        {
            if (evt.Entity != _player)
                return;

            _isUseCompleted = false;
            EventBus.Raise(new StopPlayerGageEvent());
        }
    }
}

