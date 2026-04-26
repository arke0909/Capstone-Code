using Code.Hotbar;
using Code.InventorySystem;
using Code.InventorySystems.Items;
using Code.UI.Core;
using InGame.InventorySystem;
using TMPro;
using UnityEngine;
using static Code.InventorySystems.InventoryUtility;

namespace Code.InGame.Hotbar
{
    public class HotbarSlotUI : MonoBehaviour, IUIElement<HotbarSlot>
    {
        [SerializeField] private int indexOffset = 3;
        [SerializeField] private ItemSlotUI slotUI;
        [SerializeField] private TextMeshProUGUI indexText;
        
        [field: SerializeField] public HotbarType HotbarType { get; private set; }
        
        public int Index => transform.GetSiblingIndex() + indexOffset + (int)SlotType.Hotbar;
        
        public void EnableFor(HotbarSlot slot)
        {
            slotUI?.EnableFor(slot);
        }

        public void ClearUI()
        {
            slotUI?.ClearUI();
        }

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