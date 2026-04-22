// using Assets.Work.AKH.Scripts.SkillSystem.Skills;
// using Code.SHS.Enemies;
// using Code.SHS.Enemies.Behaviors;
// using Scripts.SkillSystem;
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Work.LKW.Code.EnemySpawn;
// using Object = UnityEngine.Object;
//
// namespace Scripts.Enemies.EnemyBehaviours
// {
//     public class EnemySkillManager : MonoBehaviour
//     {
//         private readonly Enemy _enemy;
//         private readonly ActiveSkillComponent _skillComponent;
//         private readonly Transform _behaviourRoot;
//         private readonly Object _logContext;
//
//         private readonly List<UseSkillBehavior> _runtimeSkillBehaviours = new();
//         private readonly HashSet<ActiveSlotType> _configuredSkillSlots = new();
//
//         private EnemySO _currentEnemyData;
//         private bool _suppressSkillChangedCallback;
//
//         public event Action OnSkillBehavioursChanged;
//         public IReadOnlyList<UseSkillBehavior> RuntimeSkillBehaviours => _runtimeSkillBehaviours;
//
//         public EnemySkillManager(Enemy enemy, ActiveSkillComponent skillComponent, Transform behaviourRoot, Object logContext)
//         {
//             _enemy = enemy;
//             _skillComponent = skillComponent;
//             _behaviourRoot = behaviourRoot;
//             _logContext = logContext;
//
//             if (_skillComponent != null)
//                 _skillComponent.OnSkillsChanged += HandleSkillsChanged;
//         }
//
//         public void Dispose()
//         {
//             if (_skillComponent != null)
//                 _skillComponent.OnSkillsChanged -= HandleSkillsChanged;
//         }
//
//         public void UpdateConfiguredSkillSlots(IEnumerable<UseSkillBehavior> configuredSkillBehaviours)
//         {
//             _configuredSkillSlots.Clear();
//
//             if (configuredSkillBehaviours == null)
//                 return;
//
//             foreach (UseSkillBehavior behaviour in configuredSkillBehaviours)
//             {
//                 if (behaviour == null)
//                     continue;
//
//                 _configuredSkillSlots.Add(behaviour.SlotType);
//             }
//         }
//
//         public void HandleSpawn(EnemySO enemyData)
//         {
//             _currentEnemyData = enemyData;
//             RebuildSkillsForEnemy(_currentEnemyData);
//             RebuildRuntimeSkillBehaviours();
//             OnSkillBehavioursChanged?.Invoke();
//         }
//
//         private void HandleSkillsChanged()
//         {
//             if (_suppressSkillChangedCallback)
//                 return;
//
//             RebuildRuntimeSkillBehaviours();
//             OnSkillBehavioursChanged?.Invoke();
//         }
//
//         private void RebuildSkillsForEnemy(EnemySO enemyData)
//         {
//             if (_skillComponent == null)
//                 return;
//
//             _suppressSkillChangedCallback = true;
//             try
//             {
//                 if (enemyData == null)
//                 {
//                     _skillComponent.SetSkills(_skillComponent.GetComponentsInChildren<ActiveSkill>(true));
//                     return;
//                 }
//
//                 EnemySkillConfig[] skillConfigs = enemyData.skillConfigs;
//                 if (skillConfigs.Length > 0)
//                 {
//                     //_skillComponent.DestroyAllActiveSkills();
//                     List<ActiveSkill> spawnedSkills = SpawnSkills(skillConfigs);
//                     _skillComponent.SetSkills(spawnedSkills);
//                 }
//                 else
//                 {
//                     _skillComponent.SetSkills(_skillComponent.GetComponentsInChildren<ActiveSkill>(true));
//                 }
//             }
//             finally
//             {
//                 _suppressSkillChangedCallback = false;
//             }
//         }
//
//         private List<ActiveSkill> SpawnSkills(IEnumerable<EnemySkillConfig> skillConfigs)
//         {
//             List<ActiveSkill> spawnedSkills = new();
//
//             foreach (EnemySkillConfig skillConfig in skillConfigs)
//             {
//                 if (skillConfig == null || skillConfig.skillPrefab == null)
//                     continue;
//
//                 GameObject spawnedObject = Object.Instantiate(skillConfig.skillPrefab, _skillComponent.transform);
//                 spawnedObject.transform.position = _behaviourRoot.position;
//                 ActiveSkill skill = spawnedObject.GetComponent<ActiveSkill>();
//
//                 if (skill == null)
//                 {
//                     Debug.LogWarning($"Spawned skill prefab [{skillConfig.skillPrefab.name}] has no ActiveSkill component.", _logContext);
//                     Object.Destroy(spawnedObject);
//                     continue;
//                 }
//
//                 ApplySkillConfig(skill, skillConfig);
//                 skill.canUse = true;
//                 spawnedSkills.Add(skill);
//             }
//
//             return spawnedSkills;
//         }
//
//         private static void ApplySkillConfig(ActiveSkill skill, EnemySkillConfig skillConfig)
//         {
//             if (skill == null || skillConfig == null || skillConfig.targetState == null)
//                 return;
//
//             if (skill is IUseStateSkill useStateSkill)
//                 useStateSkill.TargetState = skillConfig.targetState;
//         }
//
//         private void RebuildRuntimeSkillBehaviours()
//         {
//
//             if (_skillComponent == null || _skillComponent.Sockets == null)
//                 return;
//
//             foreach (KeyValuePair<ActiveSlotType, ActiveSkillSocket> pair in _skillComponent.Sockets)
//             {
//                 ActiveSlotType slotType = pair.Key;
//                 ActiveSkillSocket skillSocket = pair.Value;
//
//                 if (skillSocket?.CurrentActiveSkill == null)
//                     continue;
//
//                 if (_configuredSkillSlots.Contains(slotType))
//                     continue;
//
//                 GameObject behaviourObject = new GameObject($"UseSkillBehaviour_{slotType}");
//                 behaviourObject.transform.SetParent(_behaviourRoot, false);
//
//                 UseSkillBehavior skillBehaviour = behaviourObject.AddComponent<UseSkillBehavior>();
//
//                 skillBehaviour.Init(_enemy);
//
//                 _runtimeSkillBehaviours.Add(skillBehaviour);
//             }
//         }
//         private static int GetDefaultSkillPriority(ActiveSlotType slotType)
//         {
//             return slotType switch
//             {
//                 ActiveSlotType.Space => 15,
//                 ActiveSlotType.E => 14,
//                 ActiveSlotType.C => 13,
//                 _ => 10
//             };
//         }
//     }
// }
