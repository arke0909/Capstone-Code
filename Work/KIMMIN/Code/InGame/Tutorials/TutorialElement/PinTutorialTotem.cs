using Code.UI.Core;
using Scripts.Entities;
using Scripts.GameSystem;
using UnityEngine;

namespace Work.Code.Tutorials
{
    public class PinTutorialTotem : InteractableStructure
    {
        [SerializeField] private UIBase pinTutorialUI;
        
        public override void Interact(Entity interactor)
        {
            pinTutorialUI.EnableUI();
        }

        public override void DeSelect()
        {
            base.DeSelect();
            pinTutorialUI.DisableUI();
        }
    }
}