using System;
using System.Reflection.Emit;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Code.InventorySystems.Items;
using Code.Players;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace InGame.InventorySystem
{
    public class EquipSlotUI : MonoBehaviour, IUIElement<ItemSlot>
    {
        [SerializeField] private ItemSlotUI slotUI;
        [SerializeField] private TextMeshProUGUI itemText;
        [field: SerializeField] public EquipSlotType EquipSlotType { get; private set; }
        private readonly Color _outlineColor = new Color32(100, 100, 255, 255);

        public int Index { get; private set; }
        
        private void Awake()
        {
            EventBus.Subscribe<StartDragEvent>(HandleStartDrag);
        }

        public void EnableFor(ItemSlot itemSlot)
        {
            slotUI.EnableFor(itemSlot);
        }

        private void HandleStartDrag(StartDragEvent evt)
        {
            var item = evt.ItemSlotUI.ItemSlot.Item.ItemData;
            if (item != null && item.itemType.GetEquipSlotType() == EquipSlotType)
            {
                slotUI.SetOutlineColor(_outlineColor);
                EventBus.Subscribe<EndDragEvent>(HandleEndDrag);
            }
        }
        
        private void HandleEndDrag(EndDragEvent evt)
        {
            slotUI.SetOutlineColor(_outlineColor, true);
            EventBus.Unsubscribe<EndDragEvent>(HandleEndDrag);
        }

        public void Clear()
        {
            EventBus.Unsubscribe<StartDragEvent>(HandleStartDrag);
            slotUI.Clear();
        }
        
        public void InitUI(EquipSlotDefine equipSlotDefine)
        {
            itemText.text = equipSlotDefine.slotName;
            Index = equipSlotDefine.index + (int)SlotType.Equip;
        }
    }
}