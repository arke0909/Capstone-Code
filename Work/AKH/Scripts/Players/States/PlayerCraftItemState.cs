using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.InventorySystems;
using Code.InventorySystems.Items;
using Code.Players;
using System.Collections.Generic;
using System.Linq;
using SHS.Scripts.Effects;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.GameEvents;
using Code.Items;
using Code.Items.ItemInfo;

namespace Scripts.Players.States
{
    public class PlayerCraftItemState : PlayerState
    {
        private PlayerInventory _targetInventory;
        private CraftTreeSO _targetCraftTree;
        private Dictionary<ItemDataSO, int> _consumeItems;
        private ItemDataSO[] _autoCraftedItems;
        private CraftEffect _craftEffect;

        public PlayerCraftItemState(ComponentContainer container, int animationHash) : base(container, animationHash)
        {
            _craftEffect = container.Get<CraftEffect>();
        }

        public override void Enter()
        {
            base.Enter();
            CraftContext context = _blackboard.GetOrDefault<CraftContext>("SelectedCraftSO");
            (_targetInventory, _targetCraftTree) = context;
            _consumeItems = context?.ConsumeItems ?? _targetCraftTree?.ConsumeItems;
            _autoCraftedItems = context?.AutoCraftedItems;
            Debug.Assert(_targetInventory != null || _targetCraftTree != null,
                $"{_targetInventory}, {_targetCraftTree}");
            if (_consumeItems == null || !_targetInventory.CanConsume(_consumeItems))
            {
                Debug.Log("Need More materials");
                _player.ChangeState(PlayerStateEnum.Idle);
                return;
            }
            _craftEffect.StartCrafting();

            if (!CanAddCraftResult())
            {
                Debug.Log("Not enough inventory space");
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
            if (_player.PlayerInput.MovementKey.sqrMagnitude > 0f || _player.PlayerInput.AimKey)
            {
                EventBus.Raise(new StopPlayerGageEvent());
                _player.ChangeState(PlayerStateEnum.Idle);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _craftEffect.StopCrafting();
        }

        private void HandleCompleteCraft()
        {
            EquipableItem skillSource = FindSkillSourceItem();
            ItemCreateData result = _targetCraftTree.Item.CreateItem();

            if (result.Item is EquipableItem resultEquipable && skillSource != null)
                resultEquipable.CopySkillFrom(skillSource);

            _targetInventory.TryConsume(_consumeItems);
            _targetInventory.TryAddItem(result.Item, _targetCraftTree.Count);
            _player.LocalEventBus.Raise(new CompleteCraftingEvent(result.Item.ItemData, _autoCraftedItems));
            _blackboard.Remove("SelectedCraftSO");
            _player.ChangeState(PlayerStateEnum.Idle);
        }

        private bool CanAddCraftResult()
        {
            return _targetInventory.CanAddItemAfterConsume(_targetCraftTree.Item, _targetCraftTree.Count, _consumeItems);
        }

        private EquipableItem FindSkillSourceItem()
        {
            List<NodeData> consumeNodes = _targetCraftTree.nodeList.ToList();
            consumeNodes.Remove(_targetCraftTree.Root);
            EquipableItem explicitSource =
                FindSkillSourceItem(consumeNodes.Where(node => node.InheritSkillToCraftResult));

            if (explicitSource != null)
                return explicitSource;

            return FindSkillSourceItem(consumeNodes);
        }

        private EquipableItem FindSkillSourceItem(IEnumerable<NodeData> nodes)
        {
            foreach (NodeData node in nodes)
            {
                ItemSlot sourceSlot = _targetInventory
                    .GetItemSlots(node.Item)
                    .FirstOrDefault(slot => slot.Item is EquipableItem equipableItem && equipableItem.Skill != null);

                if (sourceSlot?.Item is EquipableItem sourceItem)
                    return sourceItem;
            }

            return null;
        }
    }
}
