using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Chipmunk.ComponentContainers;
using Code.GameEvents;
using Chipmunk.GameEvents;
using Code.InventorySystems.Items;
using DewmoLib.Dependencies;
using InGame.InventorySystem;
using Scripts.Players;
using Scripts.SkillSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.Code.UI;
using Work.LKW.Code.Items;

namespace Code.UI.Inventory
{
    public class InventoryPanel : AbstractSlotUIsPanel
    {
        [SerializeField] private SkillUpgradeUI skillUpgradeUI;
        [SerializeField] private TextMeshProUGUI bagTitleText;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private bool isPlayerInventory;

        [Inject] private Player _player;
        private List<ItemSlot> _slots;
        private int _currentSlotCnt;
        private const int MinScrollSize = 30;

        protected override void Awake()
        {
            base.Awake();
            EventBus.Subscribe<UpdateInventoryUIEvent>(HandleUpdateInventoryUI);
            UpdateSlotUI();
        }
        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<UpdateInventoryUIEvent>(HandleUpdateInventoryUI);
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

        private void HandleUpdateInventoryUI(UpdateInventoryUIEvent evt)
        {
            if(evt.isPlayerInventory != isPlayerInventory) return;
            
            // 실제 인벤토리에서 받아온 데이터들
            _slots = evt.ItemSlots;
            _currentSlotCnt = evt.SlotCnt;

            int existItemSlotCnt = _slots.Count(slot => slot.Item != null);
            bagTitleText.SetText($"용량 ({existItemSlotCnt} / {_currentSlotCnt})");
            UpdateSlotUI();
        }

        protected override void HandleClick(ItemSlot slot)
        {
            if (slot.Item is EquipableItem item && isPlayerInventory)
            {
                skillUpgradeUI.EnableFor(item);
            }
        }
    }
}