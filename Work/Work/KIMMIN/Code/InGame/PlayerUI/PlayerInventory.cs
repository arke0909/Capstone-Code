using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.UI.Core;
using UnityEngine;
using Work.Code.UI;

namespace InGame.PlayerUI
{
    public class PlayerInventory : UIPanel
    {
        [SerializeField] PlayerInputSO playerInput;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject hotbarUI;
        [SerializeField] private GameObject lootSlotUI;
        private bool _withLoot;
        
        protected override void Awake()
        {
            base.Awake();
            playerInput.OnInventoryPressed += HandleInventoryPressed;
            EventBus.Subscribe<OpenPlayerUIEvent>(HandleOpenPlayerUIEvent);
            DisableUI();
        }

        private void HandleInventoryPressed()
        {
            ToggleUI(true);
        }

        private void HandleOpenPlayerUIEvent(OpenPlayerUIEvent evt)
        {
            ToggleUI(true);
            _withLoot = evt.WithLootInventory;
            UIUtility.FadeUI(lootSlotUI.gameObject, 0.1f, !IsActive);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            playerInput.OnInventoryPressed -= HandleInventoryPressed;
            EventBus.Unsubscribe<OpenPlayerUIEvent>(HandleOpenPlayerUIEvent);
        }

        public override void ToggleUI(bool hasTween = false)
        {
            base.ToggleUI(hasTween);
            
            if (_withLoot)
            {
                _withLoot = false; 
                UIUtility.FadeUI(lootSlotUI.gameObject, 0.1f, !IsActive);
            }
        }

        public override void DisableUI(bool isFade = false)
        {
            base.DisableUI(isFade);
            UIUtility.FadeUI(lootSlotUI.gameObject, 0.1f, true);
            playerInput.SetPlayerInput(true);
        }
    }
}