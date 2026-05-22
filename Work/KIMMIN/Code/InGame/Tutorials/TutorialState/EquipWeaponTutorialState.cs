using Chipmunk.ComponentContainers;
using Code.Items.ItemInfo;
using Code.Players;
using Code.UI.Inventory;
using Scripts.Players;
using UnityEngine;
using PlayerInventory = InGame.PlayerUI.PlayerInventory;

namespace Work.Code.Tutorials
{
    public class EquipWeaponTutorialState : TutorialState
    {
        [SerializeField] private LeftInventoryPanel playerInventory;
        private PlayerEquipment _playerEquipment;
        
        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            _playerEquipment = _player.Get<PlayerEquipment>();
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            _playerEquipment.OnEquipItem += HandleEquipItem;
            //playerInventory.HighlightSlot(ItemType.Gun, Color.white);
        }

        private void HandleEquipItem()
        {
            TutorialComplete();
        }

        public override void ExitTutorial()
        {
            _playerEquipment.OnEquipItem -= HandleEquipItem;
            playerInventory.StopAllHighlightSlots();
        }
    }
}