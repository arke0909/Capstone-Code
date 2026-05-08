using System;
using Scripts.Players;
using UnityEngine;
using Work.LKW.Code.ItemContainers;

namespace Work.Code.Tutorials
{
    public class FarmingTutorialState : TutorialState
    {
        [SerializeField] private TutorialMarking[] markings;
        [SerializeField] private ItemContainer[] containers;
        [SerializeField] private GameObject[] arrows;

        private bool[] _conditions;
        private Action[] _handlers;

        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            
            _conditions = new bool[containers.Length];
            _handlers = new Action[containers.Length];
            
            SetArrows(false);
            SetMarking(false);
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();

            for(int i = 0; i < containers.Length; i++)
            {
                int idx = i;
                _handlers[i] = () => HandleEmptyInventory(idx);
                containers[i].Inventory.InventoryEmpty += _handlers[i];
            }
            
            SetMarking(true);
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
            
            for (int i = 0; i < containers.Length; i++)
            {
                containers[i].Inventory.InventoryEmpty -= _handlers[i];
            }
            
            SetMarking(false);
            SetArrows(false);
        }

        private void SetArrows(bool state)
        {
            foreach (var arrow in arrows)
            {
                arrow.SetActive(state);
            }
        }
        
        private void SetMarking(bool state)
        {
            foreach (var marking in markings)
            {
                marking.SetEnable(state);
            }
        }
    }
}