using Chipmunk.ComponentContainers;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.Hotbar;
using Code.InventorySystem;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.PlayerTasks
{
    public class EquipHotbarTask : PlayerTask
    {
        [SerializeField] private HotbarInventoryUI hotbarInventory;

        public override void StartTask()
        {
            base.StartTask();
            EventBus.Subscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
            
            foreach (var hotbar in hotbarInventory.GetHotbarSlots())
            {
                hotbar.PlayBackgroundEffect(Color.white);
            }
        }

        private void HandleUpdateHotbar(UpdateHotbarUIEvent evt)
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            EventBus.Unsubscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
            
            foreach (var hotbar in hotbarInventory.GetHotbarSlots())
            {
                hotbar.StopBackgroundEffect();
            }
        }

        protected override string GetTaskText()
        {
            return "붕대를 드래그 하여 핫바에 장착하세요.";
        }
    }
}