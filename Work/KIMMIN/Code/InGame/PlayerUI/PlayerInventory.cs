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
        [SerializeField] private UIBase lootSlotUI;
        private bool _withLoot;
        
        protected override void Awake()
        {
            base.Awake();

            playerInput.OnInventoryPressed += HandleInventoryPressed;
            EventBus.Subscribe<OpenPlayerUIEvent>(HandleOpenPlayerUIEvent);
        }

        private void Start()
        {
            ForceHideLootUI();
        }

        private void HandleInventoryPressed()
        {
            if (IsActive)
            {
                ToggleUI(true);
                return;
            }

            ForceHideLootUI();
            OpenInventoryPanel();
        }

        private void HandleOpenPlayerUIEvent(OpenPlayerUIEvent evt)
        {
            OpenInventoryPanel();

            if (evt.WithLootInventory)
                OpenLootUI();
            else
                ForceHideLootUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            playerInput.OnInventoryPressed -= HandleInventoryPressed;
            EventBus.Unsubscribe<OpenPlayerUIEvent>(HandleOpenPlayerUIEvent);
        }

        public override void ToggleUI(bool hasTween = false)
        {
            bool willClose = IsActive;

            base.ToggleUI(hasTween);

            if (willClose || !_withLoot)
                ForceHideLootUI();
        }

        public override void DisableUI(bool isFade = false)
        {
            ForceHideLootUI();
            base.DisableUI(isFade);
        }

        private void OpenInventoryPanel()
        {
            if (!IsActive)
                base.ToggleUI(true);
        }

        private void OpenLootUI()
        {
            _withLoot = true;

            if (lootSlotUI == null)
                return;

            lootSlotUI.ShowUIOnInspector();
        }

        private void ForceHideLootUI()
        {
            _withLoot = false;

            if (lootSlotUI == null)
                return;

            lootSlotUI.HideUIOnInspector();
        }
    }
}