using System;
using System.Collections.Generic;
using System.Linq;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InGame.Hotbar;
using Code.InventorySystem;
using Code.UI.Inventory;

namespace Code.Hotbar
{
    public class HotbarInventoryUI : AbstractSlotUIsPanel
    {
        private List<HotbarSlot> _hotbarSlots;
        private Dictionary<(HotbarType, int), HotbarSlotUI> _slotUis;

        protected override void Awake()
        {
            base.Awake();
            
            _slotUis = GetComponentsInChildren<HotbarSlotUI>()
                .ToDictionary(s => (s.HotbarType, s.Index), s => s);
            
            EventBus.Subscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
            base.OnDestroy();
        }

        protected override void UpdateSlotUI()
        {
            foreach (HotbarSlotUI slotUI in _slotUis.Values)
            {
                slotUI.ClearUI();
            }

            foreach (HotbarSlot hotbarSlot in _hotbarSlots)
            {
                if (_slotUis.TryGetValue((hotbarSlot.HotbarType, hotbarSlot.Index), 
                        out HotbarSlotUI ui))
                {
                    ui.EnableFor(hotbarSlot);
                }
            }
        }

        private void HandleUpdateHotbar(UpdateHotbarUIEvent evt)
        {
            _hotbarSlots = evt.EquipSlots.ToList();
            UpdateSlotUI();
        }
    }
}