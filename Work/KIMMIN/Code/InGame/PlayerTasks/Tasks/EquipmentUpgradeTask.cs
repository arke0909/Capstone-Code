using Chipmunk.ComponentContainers;
using Code.Items.ItemInfo;
using Code.Players;
using Scripts.Players;
using UnityEngine;
using Work.Code.Craft;
using Work.Code.GameEvents;

namespace Work.Code.PlayerTasks
{
    public class EquipmentUpgradeTask : PlayerTask
    {
        [SerializeField] private Rarity targetRarity;

        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            _player.LocalEventBus.Subscribe<CompleteCraftingEvent>(HandleCompleteCrafting);
        }

        private void HandleCompleteCrafting(CompleteCraftingEvent evt)
        {
            if ((evt.CraftedItem.itemType == ItemType.Armor ||
                 evt.CraftedItem.itemType == ItemType.Helmet ||
                 evt.CraftedItem.itemType == ItemType.Gun)
                && evt.CraftedItem.rarity >= targetRarity)
            {
                CompleteTask();
            }
        }

        protected override void StopTask()
        {
            _player.LocalEventBus.Unsubscribe<CompleteCraftingEvent>(HandleCompleteCrafting);
        }

        protected override string GetTaskText()
        {
            return $"R 등급 장비를 제작하세요. \n(업그레이드 시 장비 장착을 해제해야합니다.)";
        }
    }
}