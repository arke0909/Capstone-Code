using AYellowpaper.SerializedCollections;
using Chipmunk.Modules.StatSystem;
using Code.InventorySystems.Equipments;
using Code.Players;
using Code.SHS.Entities.Enemies;
using Code.SHS.Utility.DynamicFieldBinding;
using DewmoLib.ObjectPool.RunTime;
using Scripts.Combat.Datas;
using Scripts.Enemies.EnemyBehaviours;
using Scripts.FSM;
using Scripts.SkillSystem;
using Scripts.SkillSystem.Manage;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using SHS.Scripts.Entities.Levels.Growths;
using UnityEngine;
using UnityEngine.Serialization;
using Code.Items.ItemInfo;

namespace Code.EnemySpawn
{
    [Serializable]
    public struct EnemyEquipData
    {
        public EquipPartType partType;
        public EquipItemDataSO itemData;
    }

    public enum EnemyType
    {
        Boss,
        Shadow,
        Common
    }

    [CreateAssetMenu(fileName = "Enemy Data", menuName = "SO/EnemySpawn/EnemySO", order = 0)]
    public class EnemySO : ScriptableObject
    {
        [Header("Spawn Settings")] public EnemyType enemyType = EnemyType.Common;
        public GameObject enemyPrefab;
        public PoolItemSO enemyPoolItem;
        public int spawnRarityWeight;
        public GrowthTableSO growthTable;


        [Header("Equipment Settings")] public EnemyEquipData[] equipments;
        public BulletDataSO bulletData;

        [InlineButton("LoadStatsFromPrefab", "Load From Prefab")]
        public List<StatOverride> statOverrides;

        public StateDataSO[] stateDatas;
        [Header("Behavior Settings")] public FieldPatch<EnemyBehaviour>[] behaviourPrefabs;

        [Header("Skill Settings")] [SerializeField]
        public SerializedDictionary<PassiveSlotType, FieldPatch<PassiveSkill>> passiveSkill = new();

        [SerializeField] public SerializedDictionary<ActiveSlotType, FieldPatch<ActiveSkill>> activeSkill = new();

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

    public static class EnemySpawnUtility
    {
        public static Enemy SpawnEnemy(EnemySO enemyData, Vector3 position, Quaternion rotation,
            PoolManagerMono poolManager = null)
        {
            if (enemyData == null)
            {
                Debug.LogWarning("Tried to spawn an enemy with null EnemySO.");
                return null;
            }

            Enemy enemy = TrySpawnFromPool(enemyData, poolManager);
            if (enemy == null)
            {
                if (enemyData.enemyPrefab == null)
                {
                    Debug.LogWarning($"EnemySO {enemyData.name} has no enemy prefab assigned.");
                    return null;
                }

                GameObject enemyObject = UnityEngine.Object.Instantiate(enemyData.enemyPrefab, position, rotation);
                enemy = GetEnemyComponent(enemyData, enemyObject);
                if (enemy == null)
                {
                    Debug.LogError($"Enemy prefab {enemyData.enemyPrefab.name} does not have an Enemy component.");
                    UnityEngine.Object.Destroy(enemyObject);
                    return null;
                }
            }
            else
            {
                enemy = GetEnemyComponent(enemyData, enemy.gameObject);
                if (enemy == null)
                    return null;

                enemy.transform.SetPositionAndRotation(position, rotation);
            }

            enemy.SetRuntimePoolItem(enemyData.enemyPoolItem);
            enemy.SpawnEnemy(position, enemyData);
            return enemy;
        }

        private static Enemy TrySpawnFromPool(EnemySO enemyData, PoolManagerMono poolManager)
        {
            if (enemyData.enemyPoolItem == null)
                return null;

            poolManager ??= UnityEngine.Object.FindAnyObjectByType<PoolManagerMono>();
            if (poolManager == null)
            {
                Debug.LogWarning(
                    $"PoolManagerMono could not be found. Falling back to Instantiate for enemy {enemyData.name}.");
                return null;
            }

            Enemy enemy = poolManager.Pop<Enemy>(enemyData.enemyPoolItem);
            if (enemy == null)
            {
                Debug.LogWarning(
                    $"Pool item {enemyData.enemyPoolItem.name} returned null for enemy {enemyData.name}. Falling back to Instantiate.");
            }

            return enemy;
        }

        private static Enemy GetEnemyComponent(EnemySO enemyData, GameObject enemyObject)
        {
            if (enemyObject == null)
                return null;

            if (TryGetExpectedEnemyComponent(enemyData.enemyType, enemyObject, out Enemy typedEnemy))
                return typedEnemy;

            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy == null)
                return null;

            if (enemyData.enemyType == EnemyType.Boss && enemy is not Boss)
            {
                Debug.LogWarning(
                    $"EnemySO {enemyData.name} is marked as Boss, but prefab {enemyObject.name} does not have a Boss component. Using {enemy.GetType().Name} instead.",
                    enemyObject);
            }

            return enemy;
        }

        private static bool TryGetExpectedEnemyComponent(EnemyType enemyType, GameObject enemyObject, out Enemy enemy)
        {
            switch (enemyType)
            {
                case EnemyType.Boss:
                    if (enemyObject.TryGetComponent(out Boss boss))
                    {
                        enemy = boss;
                        return true;
                    }

                    break;
                case EnemyType.Shadow:
                    if (enemyObject.TryGetComponent(out Shadow shadow))
                    {
                        enemy = shadow;
                        return true;
                    }

                    break;
                case EnemyType.Common:
                    if (enemyObject.TryGetComponent(out Enemy commonEnemy))
                    {
                        enemy = commonEnemy;
                        return true;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(enemyType), enemyType, null);
            }

            enemy = null;
            return false;
        }
    }
}
