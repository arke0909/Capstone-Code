using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Tutorials
{
    public class FarmingTutorialState : TutorialState
    {
        [SerializeField] private TutorialMarking[] markings;
        [SerializeField] private ItemDataSO[] requireItems;
        
        private PlayerInventory _playerInventory;

        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            _playerInventory = _player.Get<PlayerInventory>();
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            _playerInventory.InventoryChanged += HandleInventoryChanged;

            foreach (var marking in markings)
            {
                marking.SetVisual(true);
            }
        }

        private void HandleInventoryChanged()
        {
            TutorialComplete();
        }

        public override void ExitTutorial()
        {
            _playerInventory.InventoryChanged -= HandleInventoryChanged;
            
            foreach (var marking in markings)
            {
                marking.SetVisual(false);
            }
        }
    }
}