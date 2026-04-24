using System;
using Code.Hotbar;
using Code.InventorySystem;
using Code.UI.Core;
using InGame.InventorySystem;
using TMPro;
using UnityEngine;
using Work.LKW.Code.Items;

namespace Code.InGame.Hotbar
{
    public class HotbarSlotUI : MonoBehaviour, IUIElement<HotbarSlot>
    {
        [SerializeField] private int indexOffset = 3;
        [SerializeField] private ItemSlotUI slotUI;
        [SerializeField] private TextMeshProUGUI indexText;
        
        [field: SerializeField] public HotbarType HotbarType { get; private set; }
        
        public int Index => transform.GetSiblingIndex() + indexOffset;
        
        public void EnableFor(HotbarSlot slot)
        {
            slotUI?.EnableFor(slot);
        }

        public void Clear()
        {
            slotUI?.Clear();
        }

        private void OnValidate()
        {
            name = $"{HotbarType}_HotbarSlot_{transform.GetSiblingIndex()}";
            if (indexText != null)
                indexText.text = (Index + 1).ToString();
        }
    }
}