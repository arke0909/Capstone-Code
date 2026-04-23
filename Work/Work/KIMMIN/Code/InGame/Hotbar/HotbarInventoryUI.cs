using System;
using System.Collections.Generic;
using System.Linq;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InGame.Hotbar;
using Code.InventorySystem;
using Code.UI.Inventory;
using UnityEngine;

namespace Code.Hotbar
{
    public class HotbarInventoryUI : AbstractSlotUIsPanel
    {
        private List<HotbarSlot> _equips;
        private Dictionary<(HotbarType, int), HotbarSlotUI> _slots;

        protected override void Awake()
        {
            base.Awake();
            
            _slots = GetComponentsInChildren<HotbarSlotUI>().ToDictionary(s => (s.HotbarType, s.Index), s => s);
            
            EventBus.Subscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
        }

        protected override void OnDestroy()
        {
            EventBus.Unsubscribe<UpdateHotbarUIEvent>(HandleUpdateHotbar);
            base.OnDestroy();
        }

        protected override void UpdateSlotUI()
        {
            foreach (HotbarSlotUI slotUI in _slots.Values)
            {
                slotUI.Clear();
            }

            foreach (var equip in _equips)
            {
                if (_slots.TryGetValue((equip.HotbarType, equip.Index), out var ui))
                {
                    ui.EnableFor(equip);
                }
            }
        }

        private void HandleUpdateHotbar(UpdateHotbarUIEvent evt)
        {
            _equips = evt.EquipSlots.ToList();
            UpdateSlotUI();
        }
    }
}