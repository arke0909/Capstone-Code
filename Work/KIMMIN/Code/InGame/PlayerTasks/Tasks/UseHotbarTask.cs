using Chipmunk.ComponentContainers;
using Code.InventorySystem;
using Code.Items.ItemInfo;
using Scripts.Players;
using UnityEngine;
using Work.Code.Tutorials;

namespace Work.Code.PlayerTasks
{
    public class UseHotbarTask : PlayerTask
    {
        [SerializeField] private ItemDataSO targetItem;
        [SerializeField] private TutorialDoor tutorialDoor;
        
        private PlayerHotbar playerHotbar;

        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            playerHotbar = player.Get<PlayerHotbar>();
        }

        public override void StartTask()
        {
            base.StartTask();
            
            playerHotbar.OnHotbarUse += HandleUseHotbar;
        }
        
        private void HandleUseHotbar(ItemDataSO usedItem)
        {
            if (usedItem != null && usedItem == targetItem)
            {
                CompleteTask();
            }
        }

        protected override void StopTask()
        {
            playerHotbar.OnHotbarUse -= HandleUseHotbar;
        }

        protected override string GetTaskText()
        {
            return $"핫바에 눌러 아이템을 사용하세요.";
        }
    }
}