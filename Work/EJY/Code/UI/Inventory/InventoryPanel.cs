using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Items;
using DewmoLib.Dependencies;
using InGame.InventorySystem;
using Scripts.Players;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Inventory
{
    public abstract class InventoryPanel<T>
        : AbstractSlotUIsPanel where T : IUpdateInventoryUIEvent
    {
        [SerializeField] protected SkillUpgradeUI skillUpgradeUI;
        [SerializeField] protected TextMeshProUGUI bagTitleText;
        [SerializeField] protected ScrollRect scrollRect;

        [Inject] private Player _player;
        private List<ItemSlot> _slots;
        private int _currentSlotCnt;
        private const int MinScrollSize = 30;

        protected override void Awake()
        {
            base.Awake();
            EventBus.Subscribe<T>(HandleUpdateInventoryUI);
            UpdateSlotUI();
        }
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<T>(HandleUpdateInventoryUI);
            base.OnDestroy();
        }

        public override void DisableUI(bool isFade = false)
        {
            base.DisableUI(isFade);
            skillUpgradeUI?.DisableUI();
        }

        protected override void UpdateSlotUI()
        {
            foreach (ItemSlotUI slotUI in _slotUIs)
            {
                slotUI.ClearUI();
            }

            // 현재 인벤토리 슬롯 개수만큼만 업데이트
            for (int i = 0; i < _currentSlotCnt; i++)
            {
                _slotUIs[i].gameObject.SetActive(true);
                _slotUIs[i].EnableFor(_slots[i]);
            }

            // 사용 가능한 슬롯을 제외한 나머진 끄기
            for (int i = _currentSlotCnt; i < _slotUIs.Count; i++)
            {
                _slotUIs[i].gameObject.SetActive(false);
            }

            scrollRect.vertical = _currentSlotCnt > MinScrollSize;

            if (_currentSlotCnt <= MinScrollSize)
            {
                scrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        private void HandleUpdateInventoryUI(T evt)
        {
            _slots = evt.ItemSlots;
            _currentSlotCnt = evt.SlotCnt;

            int existItemSlotCnt = _slots.Count(slot => slot.Item != null);
            bagTitleText.SetText($"용량 ({existItemSlotCnt} / {_currentSlotCnt})");
            UpdateSlotUI();
        }

        protected override void HandleClick(ItemSlot slot)
        {
        }
    }
}