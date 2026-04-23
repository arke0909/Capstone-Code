using Code.DataSystem;
using Scripts.SkillSystem;
using System;
using Chipmunk.Modules.StatSystem;
using UnityEditor;
using UnityEngine;

namespace Work.LKW.Code.Items.ItemInfo
{
    [Serializable]
    public struct AddStatData
    {
        public StatSO targetStat;
        public float value;
    }
    
    public abstract class EquipItemDataSO : ItemDataSO
    {
        public AddStatData[] addStats;
        [ExcelColumn("modelOffset")]
        public Vector3 modelOffset;
        public GameObject equipmentPrefab;
        public SkillDBSO skillDB;
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (equipmentPrefab == null)
            {
                string[] guids = AssetDatabase.FindAssets("HandleItem t:Prefab");

                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    
                    equipmentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (equipmentPrefab != null)
                    {
                        EditorUtility.SetDirty(this);
                    }
                }
            }
        }
#endif
    }
}