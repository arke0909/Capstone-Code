using System.Collections.Generic;
using System.Linq;
using Chipmunk.GameEvents;
using Code.GameEvents;
using Code.InventorySystems.Equipments;
using Code.UI.Inventory;
using UnityEngine;

namespace InGame.InventorySystem
{
    public class EquipSlotsUI : AbstractSlotUIsPanel
    {
        [SerializeField] private EquipSlotDefineListSO equipSlotDefine;
        private List<EquipSlotUI> _equipSlotUIs = new List<EquipSlotUI>();
        private List<EquipSlot> _equipSlots;

        protected override void Awake()
        {
            base.Awake();

            _equipSlotUIs = GetComponentsInChildren<EquipSlotUI>().ToList();

            for (int i = 0; i < equipSlotDefine.equipSlotDefines.Count; i++)
            {
                if (i >= _equipSlotUIs.Count) break;
                _equipSlotUIs[i].InitUI(equipSlotDefine.equipSlotDefines[i]);
            }

            EventBus<UpdateEquipUIEvent>.OnEvent += HandleUpdateUI;
        }

        protected override void OnDestroy()
        {
            EventBus<UpdateEquipUIEvent>.OnEvent -= HandleUpdateUI;
            
            base.OnDestroy();
        }

        private void HandleUpdateUI(UpdateEquipUIEvent evt)
        {
            _equipSlots = evt.EquipSlots;
            UpdateSlotUI();
        }

        protected override void UpdateSlotUI()
        {
            for (int i = 0; i < _equipSlotUIs.Count; i++)
            {
                _equipSlotUIs[i].Clear();
                var equipSlot = _equipSlots.FirstOrDefault(slot => slot.Index == _equipSlotUIs[i].Index);
                _equipSlotUIs[i].EnableFor(equipSlot);
            }
        }
    }
}