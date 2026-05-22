using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scripts.Players;
using UnityEngine;
using Work.Code.Craft.Installer;
using Work.Code.GameEvents;
using Code.Items.ItemInfo;
using Code.UI.Core;

namespace Work.Code.Tutorials
{
    public class CraftingTutorialState : TutorialState
    {
        [SerializeField] private CraftTreeUI craftTreeUI;
        [SerializeField] private ItemDataSO[] requireItems;
        [SerializeField] private TutorialDoor tutorialDoor;
        
        private List<ItemDataSO> _requiredItems = new();
        private Color _effectColor = new Color(0.5f, 0.8f, 1f);

        public override void InitializeTutorial(TutorialController tutorialController, Player player)
        {
            base.InitializeTutorial(tutorialController, player);
            _requiredItems = requireItems.ToList();
        }

        public override void EnterTutorial()
        {
            base.EnterTutorial();
            _player.LocalEventBus.Subscribe<CompleteCraftingEvent>(HandleItemCraft);
            
            foreach (var item in _requiredItems)
            {
                craftTreeUI.HighlightCraftItem(item, true, _effectColor);
            }
        }

        private void HandleItemCraft(CompleteCraftingEvent evt)
        {
            _requiredItems.Remove(evt.CraftedItem);
            _tutorialController.SetDialogue(GetDialogue(), true);
            craftTreeUI.HighlightCraftItem(evt.CraftedItem, false);
            
            if (_requiredItems.Count == 0)
            {
                craftTreeUI.DisableUI();
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
            strBuilder.Append("G키를 눌러 제작창을 열고 ");

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

        protected override void TutorialComplete()
        {
            tutorialDoor.OpenDoor();
            base.TutorialComplete();
        }
    }
}