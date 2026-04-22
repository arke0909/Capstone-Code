using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Code.InventorySystems.Equipments
{
    public enum EquipPartType
    {
        None = -1,
        Hand,
        Helmet,
        Armor,
        Count
    }

    public enum EquipSlotType
    {
        None = -1,
        Gun,
        Melee,
        Helmet,
        Armor,
        Count
    }
    
    [Serializable]
    public struct EquipSlotDefine
    {
        public string slotName;
        public EquipPartType equipPart;
        public EquipSlotType allowedEquipSlot;
        public bool canHandle;
        public int index;
    }
    
    [CreateAssetMenu(fileName = "EquipSlot Define List", menuName = "SO/Equip Define", order = 0)]
    public class EquipSlotDefineListSO : ScriptableObject
    {
        public List<EquipSlotDefine> equipSlotDefines;

        private void OnValidate()
        {
            for (int i = 0; i < equipSlotDefines.Count; i++)
            {
                var equipSlotDefine = equipSlotDefines[i];
                equipSlotDefine.index = i;
                equipSlotDefines[i] = equipSlotDefine;
            }
        }
    }
}