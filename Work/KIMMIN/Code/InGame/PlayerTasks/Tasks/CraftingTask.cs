using Code.Items.ItemInfo;
using Scripts.Players;
using UnityEngine;
using Work.Code.Craft.Installer;
using Work.Code.GameEvents;

namespace Work.Code.PlayerTasks
{
    public class CraftingTask : PlayerTask
    {
        [SerializeField] private CraftTreeUI craftTreeUI;
        [SerializeField] private ItemDataSO requireItems;

        private bool _disableOtherItems = true;

        public void InitTask(CraftTreeUI craftTreeUI, ItemDataSO requireItems, bool disableOtherItems = true)
        {
            this.craftTreeUI = craftTreeUI;
            this.requireItems = requireItems;
            _disableOtherItems = disableOtherItems;
        }

        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);

            if (requireItems == null)
            {
                Debug.LogError("Require Item is null");
                CompleteTask();
            }
        }
        
        public override void StartTask()
        {
            if (_disableOtherItems)
                craftTreeUI?.RegisterTutorialCraftItem(requireItems);
            
            _player.LocalEventBus.Subscribe<CompleteCraftingEvent>(HandleItemCraft);
            UpdateTaskText();
        }

        private void HandleItemCraft(CompleteCraftingEvent evt)
        {
            if (evt.ContainsCraftedItem(requireItems))
            {
                CompleteTask();
            }
        }

        public void CompleteCraftingTask()
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            _player.LocalEventBus.Unsubscribe<CompleteCraftingEvent>(HandleItemCraft);
            craftTreeUI?.UnregisterTutorialCraftItem(requireItems);
            craftTreeUI?.DisableUI();
        }

        protected override string GetTaskText()
        {
            return $"{requireItems.itemName}을/를 제작하세요.";
        }

        private void OnValidate()
        {
            if (requireItems != null)
            {
                name = $"{requireItems.itemName}_CraftingTask";
            }
        }
    }
}
