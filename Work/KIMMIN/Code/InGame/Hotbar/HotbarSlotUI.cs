using Code.Hotbar;
using Code.InventorySystem;
using Code.InventorySystems.Items;
using InGame.InventorySystem;
using TMPro;
using UnityEngine;
using Work.Code.UI.Slots;
using static Code.InventorySystems.InventoryUtility;

namespace Code.InGame.Hotbar
{
    public class HotbarSlotUI : BaseSlotUI
    {
        public const int IndexOffset = 2;

        [SerializeField] private ItemSlotUI slotUI;
        [SerializeField] private TextMeshProUGUI indexText;
        
        [field: SerializeField] public HotbarType HotbarType { get; private set; }
        
        public int Index => transform.GetSiblingIndex() + IndexOffset + (int)SlotType.Hotbar;
        
        public void EnableFor(HotbarSlot slot)
        {
            slotUI?.EnableFor(slot);
        }

        public void ClearUI()
        {
            slotUI?.ClearUI();
        }

        public BaseSlotUI GetSlotUI() => slotUI;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            name = $"{HotbarType}_HotbarSlot_{transform.GetSiblingIndex()}";
            if (indexText != null)
                indexText.text = GetLocalIndex(Index + 1).ToString();
        }
        #endif
    }
}