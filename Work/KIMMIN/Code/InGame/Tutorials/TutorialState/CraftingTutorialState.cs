using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scripts.Players;
using UnityEngine;
using Work.Code.GameEvents;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Tutorials
{
    public class CraftingTutorialState : TutorialState
    {
        [SerializeField] private ItemDataSO[] requireItems;
        
        private List<ItemDataSO> _requiredItems = new();

        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            _requiredItems = requireItems.ToList();
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            _player.LocalEventBus.Subscribe<CompleteCraftingEvent>(HandleItemCraft);
        }

        private void HandleItemCraft(CompleteCraftingEvent evt)
        {
            _requiredItems.Remove(evt.CraftedItem);
            _tutorialController.SetDialogue(GetDialogue(), true);
            
            if (_requiredItems.Count == 0)
            {
                TutorialComplete();
            }
        }

        public override void ExitTutorial()
        {
            _player.LocalEventBus.Unsubscribe<CompleteCraftingEvent>(HandleItemCraft);
        }

        protected override string GetDialogue()
        {
            StringBuilder strBuilder = new();

            for (int i = 0; i < _requiredItems.Count; i++)
            {
                strBuilder.Append(_requiredItems[i].itemName);
                
                if (i != _requiredItems.Count - 1)
                {
                    strBuilder.Append(", ");
                }
            }

            strBuilder.Append("을 제작하세요");
            return strBuilder.ToString();
        }
    }
}