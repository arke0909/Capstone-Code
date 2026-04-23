using AYellowpaper.SerializedCollections;
using Chipmunk.Modules.StatSystem;
using Code.Players;
using Code.SHS.Utility.DynamicFieldBinding;
using Scripts.Combat.Datas;
using Scripts.Enemies.EnemyBehaviours;
using Scripts.FSM;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Manage;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Code.InventorySystems.Equipments;
using UnityEngine;
using UnityEngine.Serialization;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.EnemySpawn
{
    [Serializable]
    public struct EnemyEquipData
    {
        public EquipPartType partType;
        public EquipItemDataSO itemData;
    }

    [CreateAssetMenu(fileName = "Enemy Data", menuName = "SO/EnemySpawn/EnemySO", order = 0)]
    public class EnemySO : ScriptableObject
    {
        public GameObject enemyPrefab;
        public int spawnRarityWeight;

        [Header("Equipment Settings")] public EnemyEquipData[] equipments;
        public BulletDataSO bulletData;

        [InlineButton("LoadStatsFromPrefab", "Load From Prefab")]
        public List<StatOverride> statOverrides;
        public StateDataSO[] stateDatas;
        [Header("Behavior Settings")] public FieldPatch<EnemyBehaviour>[] behaviourPrefabs;

        [Header("Skill Settings")]
        [SerializeField]
        public SerializedDictionary<PassiveSlotType, FieldPatch<PassiveSkill>> passiveSkill = new();

        [SerializeField]
        public SerializedDictionary<ActiveSlotType, FieldPatch<ActiveSkill>> activeSkill = new();

        private void OnEnable()
        {
            foreach (var behaviourPatch in behaviourPrefabs)
                if (behaviourPatch != null)
                    behaviourPatch.GenerateSetter();
            foreach (var skillPatch in passiveSkill.Values)
                if (skillPatch != null)
                    skillPatch.GenerateSetter();
            foreach (var skillPatch in activeSkill.Values)
                if (skillPatch != null)
                    skillPatch.GenerateSetter();
        }

        private void LoadStatsFromPrefab()
        {
            if (enemyPrefab == null)
            {
                Debug.LogWarning("Enemy Prefab is not assigned. Cannot load stats.");
                return;
            }
            var prefabStatOverrideBehavior = enemyPrefab.GetComponentInChildren<StatOverrideBehavior>();
            if (prefabStatOverrideBehavior == null)
            {
                Debug.LogWarning("No StatOverrideBehavior found in the enemy prefab. Cannot load stats.");
                return;
            }

            foreach (var prefabStatOverride in prefabStatOverrideBehavior.StatOverrides)
            {
                if (statOverrides.Contains(prefabStatOverride) == false)
                    statOverrides.Add(prefabStatOverride);
            }
        }
    }
}