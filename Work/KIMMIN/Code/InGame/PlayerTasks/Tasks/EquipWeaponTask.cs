using Chipmunk.ComponentContainers;
using Code.Items.ItemInfo;
using Code.Players;
using Code.UI.Inventory;
using Scripts.Players;
using UnityEngine;

namespace Work.Code.PlayerTasks
{
    public class EquipWeaponTask : PlayerTask
    {
        [SerializeField] private LeftInventoryPanel playerInventory;
        private PlayerEquipment _playerEquipment;
        private const string EquipWeaponTaskText = "총을 [주무기]에 장착하세요.";

        public override void InitializeTask(Player player)
        {
            base.InitializeTask(player);
            _playerEquipment = _player.Get<PlayerEquipment>();
        }

        public override void StartTask()
        {
            _playerEquipment.OnEquipItem += HandleEquipItem;
            playerInventory.HighlightSlot(ItemType.Gun, Color.white);
        }
        
        private void HandleEquipItem()
        {
            CompleteTask();
        }

        protected override void StopTask()
        {
            _playerEquipment.OnEquipItem -= HandleEquipItem;
            playerInventory.StopAllHighlightSlots();
        }

        protected override string GetTaskText()
        {
            return EquipWeaponTaskText;
        }
    }
}
