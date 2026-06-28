using Chipmunk.GameEvents;
using Code.ItemContainers;
using Scripts.Players;
using UnityEngine;
using Work.Code.GameEvents;

namespace Work.Code.PlayerTasks
{
    public class AirDropTask : PlayerTask
    {
        private ItemContainerInventory _airDropInventory;
        
        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            EventBus.Subscribe<AirdropEvent>(HandleAirDropped);
        }

        private void HandleAirDropped(AirdropEvent evt)
        {
            _airDropInventory = evt.AirDropContainer;
            _airDropInventory.InventoryChanged += HandleInventoryChanged;
        }

        private void HandleInventoryChanged()
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            _airDropInventory.InventoryChanged -= HandleInventoryChanged;
        }

        protected override string GetTaskText()
        {
            return "보급을 파밍하세요. \n(상세 위치는 맵에 표시됩니다.)";
        }
    }
}