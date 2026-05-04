using Chipmunk.ComponentContainers;
using Code.Players;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.ItemContainers;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Tutorials
{
    public class FarmingTutorialState : TutorialState
    {
        [SerializeField] private TutorialMarking[] markings;
        [SerializeField] private ItemContainer[] containers;
        [SerializeField] private GameObject[] arrows;

        private bool[] _conditions;

        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            SetArrows(false);
            
            _conditions = new bool[markings.Length];
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();

            for(int i = 0; i < containers.Length; i++)
            {
                int idx = i;
                containers[i].Inventory.InventoryEmpty += () => HandleEmptyInventory(idx);
            }

            foreach (var marking in markings)
            {
                marking.SetVisual(true);
            }
            
            SetArrows(true);
        }

        private void HandleEmptyInventory(int idx)
        {
            _conditions[idx] = true;
            CheckCondition();
        }

        private void CheckCondition()
        {
            foreach (var condition in _conditions)
            {
                if (!condition)
                    return;
            }
            
            TutorialComplete();
        }

        public override void ExitTutorial()
        {
            foreach (var marking in markings)
            {
                marking.SetVisual(false);
            }
            
            for(int i = 0; i < containers.Length; i++)
            {
                int idx = i;
                containers[i].Inventory.InventoryEmpty += () => HandleEmptyInventory(idx);
            }
            
            SetArrows(false);
        }

        private void SetArrows(bool state)
        {
            foreach (var arrow in arrows)
            {
                arrow.SetActive(state);
            }
        }
    }
}