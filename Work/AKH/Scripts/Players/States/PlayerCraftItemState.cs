using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.InventorySystems;
using Code.Players;
using System;
using System.Collections.Generic;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.GameEvents;
using Work.LKW.Code.Items.ItemInfo;

namespace Scripts.Players.States
{
    public class PlayerCraftItemState : PlayerState
    {
        private PlayerInventory _targetInventory;
        private CraftTreeSO _targetCraftTree;
        public PlayerCraftItemState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
        }
        public override void Enter()
        {
            base.Enter();
            (_targetInventory, _targetCraftTree) = _blackboard.GetOrDefault<CraftContext>("SelectedCraftSO");
            Debug.Assert(_targetInventory != null || _targetCraftTree != null, $"{_targetInventory}, {_targetCraftTree}");
            if (!_targetInventory.CanConsume(_targetCraftTree.ConsumeItems))
            {
                Debug.Log("Need More materials");
                _player.ChangeState(PlayerStateEnum.Idle);
                return;
            }
            float craftTime = _targetCraftTree.CraftTime;
            EventBus.Raise(new PlayerGageEvent("제작중", craftTime, HandleCompleteCraft));
            _player.LocalEventBus.Raise(new StartCraftingEvent());
        }
        public override void Update()
        {
            base.Update();
            if(_player.PlayerInput.MovementKey.sqrMagnitude > 0f || _player.PlayerInput.AimKey)
            {
                EventBus.Raise(new StopPlayerGageEvent());
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }
        private void HandleCompleteCraft()
        {
            ItemCreateData result = _targetCraftTree.Item.CreateItem();
            _targetInventory.TryConsume(_targetCraftTree.ConsumeItems);
            _targetInventory.TryAddItem(result.Item, _targetCraftTree.Count);
            _player.LocalEventBus.Raise(new CompleteCraftingEvent(result.Item.ItemData));
            _player.ChangeState(PlayerStateEnum.Idle);
        }
    }
}
