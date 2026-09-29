using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.Structures
{
    public class BoatKeyInsertUI : MonoBehaviour, IDropHandler
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image keyImage;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button departButton;
        [SerializeField] private PlayerInputSO playerInput;
        [SerializeField] private string guideMessage = "열쇠를 드래그하거나 아이템에 마우스를 올리고 F를 눌러 투입";

        private EndingBoat _boat;
        private ItemSlot _hoveringSlot;

        public bool IsOpen => panel != null && panel.activeSelf && _boat != null;

        private void Awake()
        {
            if (playerInput != null)
                playerInput.OnItemInteractPressed += HandleItemInteractPressed;

            EventBus.Subscribe<HoveringSlotEvent>(HandleHoveringSlot);
            EventBus.Subscribe<PlayerUIEvent>(HandlePlayerUI);

            if (departButton != null)
                departButton.onClick.AddListener(HandleDepartButton);

            Close();
        }

        private void OnDestroy()
        {
            if (playerInput != null)
                playerInput.OnItemInteractPressed -= HandleItemInteractPressed;

            EventBus.Unsubscribe<HoveringSlotEvent>(HandleHoveringSlot);
            EventBus.Unsubscribe<PlayerUIEvent>(HandlePlayerUI);

            if (departButton != null)
                departButton.onClick.RemoveListener(HandleDepartButton);
        }

        public void Open(EndingBoat boat)
        {
            if (boat == null || panel == null)
                return;

            _boat = boat;
            panel.SetActive(true);

            if (boat.IsKeyInserted)
            {
                ShowInsertedKey(boat.BoatKey != null ? boat.BoatKey.itemImage : null);
            }
            else
            {
                if (keyImage != null)
                {
                    keyImage.sprite = null;
                    keyImage.enabled = false;
                }

                if (departButton != null)
                    departButton.interactable = false;

                ShowMessage(guideMessage);
            }
        }

        public void Close()
        {
            _boat = null;
            _hoveringSlot = null;

            if (panel != null)
                panel.SetActive(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!IsOpen || eventData.pointerDrag == null)
                return;

            ItemSlotUI slotUI = eventData.pointerDrag.GetComponent<ItemSlotUI>();
            if (slotUI == null)
                slotUI = eventData.pointerDrag.GetComponentInParent<ItemSlotUI>();

            if (slotUI == null)
                return;

            _boat.TryInsertKey(slotUI.ItemSlot);
        }

        private void HandleHoveringSlot(HoveringSlotEvent evt)
        {
            if (!IsOpen)
                return;

            _hoveringSlot = evt.ItemSlot?.ItemSlot;
        }

        private void HandleItemInteractPressed()
        {
            if (!IsOpen || _hoveringSlot == null)
                return;

            _boat.TryInsertKey(_hoveringSlot);
        }

        private void HandlePlayerUI(PlayerUIEvent evt)
        {
            if (!evt.IsEnabled)
                Close();
        }

        private void HandleDepartButton()
        {
            if (!IsOpen)
                return;

            _boat.Depart();
        }

        public void ShowInsertedKey(Sprite sprite)
        {
            if (keyImage != null)
            {
                keyImage.sprite = sprite;
                keyImage.enabled = sprite != null;
            }

            if (departButton != null)
                departButton.interactable = true;

            ShowMessage("열쇠가 삽입되었다. 출발을 누르세요.");
        }

        public void ShowMessage(string message)
        {
            if (messageText != null)
                messageText.text = message;
        }
    }
}
