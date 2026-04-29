using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Players;

namespace Work.Code.Tutorials
{
    public class EquipWeaponTutorialState : TutorialState
    {
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
        }

        private void HandleEquipItem()
        {
            TutorialComplete();
        }

        public override void ExitTutorial()
        {
            _playerEquipment.OnEquipItem -= HandleEquipItem;
        }
    }
}