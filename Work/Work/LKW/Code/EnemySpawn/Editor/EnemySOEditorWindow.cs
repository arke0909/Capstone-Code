using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chipmunk.Modules.StatSystem;
using Code.SHS.Utility.DynamicFieldBinding;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.EnemySpawn.Editor
{
    public class EnemySOEditorWindow : EditorWindow
    {
        private const string WindowTitle = "EnemySO Editor";

        private static readonly MethodInfo LoadStatsFromPrefabMethod =
            typeof(EnemySO).GetMethod("LoadStatsFromPrefab", BindingFlags.Instance | BindingFlags.NonPublic);

        private EnemySO enemyTarget;
        private SerializedObject enemySerializedObject;
        private Object currentSelection;
        private Vector2 scrollPosition;

        private SerializedProperty enemyPrefabProperty;
        private SerializedProperty spawnRarityWeightProperty;
        private SerializedProperty equipmentsProperty;
        private SerializedProperty bulletDataProperty;
        private SerializedProperty statOverridesProperty;
        private SerializedProperty stateDatasProperty;
        private SerializedProperty behaviourPrefabsProperty;
        private SerializedProperty passiveSkillProperty;
        private SerializedProperty activeSkillProperty;

        private ReorderableList equipmentsList;
        private ReorderableList statOverridesList;
        private ReorderableList stateDatasList;

        private bool showCore = true;
        private bool showEquipment = true;
        private bool showStats = true;
        private bool showStates;
        private bool showBehaviours;
        private bool showSkills;

        private string actionMessage;
        private MessageType actionMessageType = MessageType.None;
        private EditorSnapshot snapshot;
        private bool snapshotDirty = true;

        [MenuItem("Tools/EnemySOEditorWindow")]
        public static void OpenFromMenu()
        {
            EnemySOEditorWindow window = GetWindow<EnemySOEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.SyncFromSelection();
            window.Show();
        }

        public static void Open(EnemySO enemy)
        {
            EnemySOEditorWindow window = GetWindow<EnemySOEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.SetTarget(enemy);
            window.Show();
            window.Focus();
        }

        [OnOpenAsset(0)]
        public static bool OpenOnDoubleClick(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is not EnemySO enemy)
            {
                return false;
            }

            Selection.activeObject = enemy;
            Open(enemy);
            return true;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
            Selection.selectionChanged += HandleSelectionChanged;
            SyncFromSelection();
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= HandleSelectionChanged;
        }

        private void OnFocus()
        {
            SyncFromSelection();
        }

        private void HandleSelectionChanged()
        {
            SyncFromSelection();
            Repaint();
        }

        private void SyncFromSelection()
        {
            currentSelection = Selection.activeObject;

            if (currentSelection is EnemySO selectedEnemy)
            {
                SetTarget(selectedEnemy);
                return;
            }

            if (enemyTarget != null)
            {
                ClearTarget();
            }
        }

        private void SetTarget(EnemySO enemy)
        {
            currentSelection = Selection.activeObject;

            if (enemyTarget == enemy && enemySerializedObject != null)
            {
                return;
            }

            enemyTarget = enemy;
            enemySerializedObject = enemy != null ? new SerializedObject(enemy) : null;
            BindProperties();
            CreateLists();
            snapshotDirty = true;
            actionMessage = null;
        }

        private void ClearTarget()
        {
            enemyTarget = null;
            enemySerializedObject = null;
            ClearProperties();
            equipmentsList = null;
            statOverridesList = null;
            stateDatasList = null;
            snapshot = null;
            snapshotDirty = true;
            actionMessage = null;
        }

        private void OnGUI()
        {
            DrawSelectionInfo();

            if (enemySerializedObject == null || enemyTarget == null)
            {
                EditorGUILayout.HelpBox("Select or double-click an EnemySO asset to edit it here.", MessageType.Info);
                return;
            }

            enemySerializedObject.UpdateIfRequiredOrScript();

            if (snapshotDirty || snapshot == null)
            {
                snapshot = BuildSnapshot(enemyTarget);
                snapshotDirty = false;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            DrawEnemyEditor();
            EditorGUILayout.EndScrollView();

            if (enemySerializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(enemyTarget);
                snapshotDirty = true;
            }
        }

        private void BindProperties()
        {
            if (enemySerializedObject == null)
            {
                ClearProperties();
                return;
            }

            enemyPrefabProperty = enemySerializedObject.FindProperty("enemyPrefab");
            spawnRarityWeightProperty = enemySerializedObject.FindProperty("spawnRarityWeight");
            equipmentsProperty = enemySerializedObject.FindProperty("equipments");
            bulletDataProperty = enemySerializedObject.FindProperty("bulletData");
            statOverridesProperty = enemySerializedObject.FindProperty("statOverrides");
            stateDatasProperty = enemySerializedObject.FindProperty("stateDatas");
            behaviourPrefabsProperty = enemySerializedObject.FindProperty("behaviourPrefabs");
            passiveSkillProperty = enemySerializedObject.FindProperty("passiveSkill");
            activeSkillProperty = enemySerializedObject.FindProperty("activeSkill");

            behaviourPrefabsProperty.isExpanded = false;
            passiveSkillProperty.isExpanded = false;
            activeSkillProperty.isExpanded = false;
        }

        private void ClearProperties()
        {
            enemyPrefabProperty = null;
            spawnRarityWeightProperty = null;
            equipmentsProperty = null;
            bulletDataProperty = null;
            statOverridesProperty = null;
            stateDatasProperty = null;
            behaviourPrefabsProperty = null;
            passiveSkillProperty = null;
            activeSkillProperty = null;
        }

        private void DrawEnemyEditor()
        {
            DrawScriptField();
            EditorGUILayout.Space(4f);
            DrawHeroCard();
            EditorGUILayout.Space(6f);
            DrawQuickActions();
            DrawActionMessage();
            DrawValidationSummary();
            EditorGUILayout.Space(4f);

            DrawSection("Core", GetCoreSummary(), new Color(0.24f, 0.56f, 0.92f), ref showCore, DrawCoreSection);
            DrawSection("Equipment", GetEquipmentSummary(), new Color(0.87f, 0.58f, 0.22f), ref showEquipment, DrawEquipmentSection);
            DrawSection("Stats", GetStatsSummary(), new Color(0.24f, 0.72f, 0.46f), ref showStats, DrawStatsSection);
            DrawSection("States", GetStatesSummary(), new Color(0.18f, 0.68f, 0.78f), ref showStates, DrawStatesSection);
            DrawSection("Behaviours", GetBehavioursSummary(), new Color(0.90f, 0.36f, 0.32f), ref showBehaviours, DrawBehavioursSection);
            DrawSection("Skills", GetSkillsSummary(), new Color(0.92f, 0.74f, 0.24f), ref showSkills, DrawSkillsSection);
        }

        private void DrawSelectionInfo()
        {
            Object selection = currentSelection;
            string selectedName = selection != null ? selection.name : "None";
            string selectedType = selection != null ? selection.GetType().Name : "No Selection";
            string selectedPath = selection != null ? AssetDatabase.GetAssetPath(selection) : string.Empty;

            using (new EditorGUILayout.VerticalScope(Styles.SelectionCard))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Selection", Styles.ToolbarTitle, GUILayout.Width(72f));
                    EditorGUILayout.LabelField($"{selectedName} ({selectedType})", Styles.ContextLabel);

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(enemyTarget == null))
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(54f)))
                        {
                            EditorGUIUtility.PingObject(enemyTarget);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    EditorGUILayout.LabelField(selectedPath, Styles.SelectionPath);
                }
            }
        }

        private void DrawScriptField()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                MonoScript script = enemyTarget != null ? MonoScript.FromScriptableObject(enemyTarget) : null;
                EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            }
        }

        private void DrawHeroCard()
        {
            using (new EditorGUILayout.VerticalScope(Styles.HeroCard))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect previewRect = GUILayoutUtility.GetRect(50f, 50f, GUILayout.Width(50f), GUILayout.Height(50f));
                    DrawPreview(previewRect, enemyTarget);

                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Space(2f);
                        GUILayout.Label(enemyTarget != null ? enemyTarget.name : "EnemySO", Styles.HeroTitle);
                        GUILayout.Label(snapshot.HeroSubtitle, Styles.HeroSubtitle);
                        GUILayout.Label(snapshot.HeroStatusLine, Styles.HeroStatus);
                    }

                    GUILayout.FlexibleSpace();

                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(84f)))
                    {
                        GUILayout.Label("Rarity", Styles.SideLabel);
                        EditorGUILayout.PropertyField(spawnRarityWeightProperty, GUIContent.none);
                    }
                }

                EditorGUILayout.Space(6f);

                DrawMetricRow("Equip", snapshot.EquipmentCount.ToString(), "Stats", snapshot.StatCount.ToString(), "States", snapshot.StateCount.ToString());
                DrawMetricRow("Behaviours", snapshot.BehaviourCount.ToString(), "Passive", snapshot.PassiveSkillCount.ToString(), "Active", snapshot.ActiveSkillCount.ToString());
            }
        }

        private void DrawQuickActions()
        {
            using (new EditorGUILayout.VerticalScope(Styles.ToolbarCard))
            {
                EditorGUILayout.LabelField("Quick Actions", Styles.ToolbarTitle);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Load Stats From Prefab", GUILayout.Height(24f)))
                    {
                        HandleLoadStatsFromPrefab();
                    }

                    if (GUILayout.Button("Sync All Patches", GUILayout.Height(24f)))
                    {
                        HandleSyncAllPatches();
                    }

                    if (GUILayout.Button("Generate Setters", GUILayout.Height(24f)))
                    {
                        HandleGenerateSetters();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Expand All Sections", GUILayout.Height(22f)))
                    {
                        SetAllSectionsExpanded(true);
                    }

                    if (GUILayout.Button("Collapse All Sections", GUILayout.Height(22f)))
                    {
                        SetAllSectionsExpanded(false);
                    }
                }
            }
        }

        private void DrawActionMessage()
        {
            if (string.IsNullOrWhiteSpace(actionMessage))
            {
                return;
            }

            EditorGUILayout.HelpBox(actionMessage, actionMessageType);
        }

        private void DrawValidationSummary()
        {
            List<string> warnings = snapshot.Warnings;
            if (warnings.Count == 0)
            {
                return;
            }

            string message = string.Join("\n", warnings.Select(warning => $"- {warning}"));
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        private void DrawCoreSection()
        {
            EditorGUILayout.PropertyField(enemyPrefabProperty);
            EditorGUILayout.PropertyField(bulletDataProperty);
        }

        private void DrawEquipmentSection()
        {
            equipmentsList.DoLayoutList();
        }

        private void DrawStatsSection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"Enabled Overrides: {snapshot.EnabledOverrideCount} / {snapshot.StatCount}",
                    Styles.ContextLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Load From Prefab", GUILayout.Width(140f)))
                {
                    HandleLoadStatsFromPrefab();
                }
            }

            EditorGUILayout.Space(2f);
            statOverridesList.DoLayoutList();
        }

        private void DrawStatesSection()
        {
            stateDatasList.DoLayoutList();
        }

        private void DrawBehavioursSection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"Configured Targets: {snapshot.ConfiguredBehaviourCount} / {snapshot.BehaviourCount}",
                    Styles.ContextLabel);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Sync", GUILayout.Width(72f)))
                {
                    HandleSyncAllPatches();
                }

                if (GUILayout.Button("Generate", GUILayout.Width(84f)))
                {
                    HandleGenerateSetters();
                }
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.PropertyField(behaviourPrefabsProperty, includeChildren: behaviourPrefabsProperty.isExpanded);
        }

        private void DrawSkillsSection()
        {
            DrawSkillPanel("Passive Slots", $"{snapshot.PassiveSkillCount} configured", passiveSkillProperty);
            EditorGUILayout.Space(6f);
            DrawSkillPanel("Active Slots", $"{snapshot.ActiveSkillCount} configured", activeSkillProperty);
        }

        private void DrawSkillPanel(string title, string summary, SerializedProperty property)
        {
            using (new EditorGUILayout.VerticalScope(Styles.SubSectionCard))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(title, Styles.SubSectionTitle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(summary, Styles.SubSectionSummary);
                }

                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(property, includeChildren: property.isExpanded);
            }
        }

        private void DrawSection(string title, string summary, Color accentColor, ref bool expanded, Action body)
        {
            using (new EditorGUILayout.VerticalScope(Styles.SectionCard))
            {
                Rect headerRect = EditorGUILayout.GetControlRect(false, 26f);
                DrawSectionHeader(headerRect, title, summary, accentColor, ref expanded);

                if (expanded)
                {
                    EditorGUILayout.Space(4f);
                    body?.Invoke();
                }
            }

            EditorGUILayout.Space(8f);
        }

        private static void DrawSectionHeader(Rect rect, string title, string summary, Color accentColor, ref bool expanded)
        {
            EditorGUI.DrawRect(rect, Styles.SectionHeaderBackground);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), accentColor);

            Rect foldoutRect = new Rect(rect.x + 10f, rect.y + 3f, rect.width - 120f, rect.height - 6f);
            Rect summaryRect = new Rect(rect.xMax - 112f, rect.y + 4f, 104f, rect.height - 8f);

            expanded = EditorGUI.Foldout(foldoutRect, expanded, title, true, Styles.SectionFoldout);
            EditorGUI.LabelField(summaryRect, summary, Styles.SectionSummary);
        }

        private void DrawMetricRow(
            string firstLabel,
            string firstValue,
            string secondLabel,
            string secondValue,
            string thirdLabel,
            string thirdValue)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawMetricChip(firstLabel, firstValue);
                DrawMetricChip(secondLabel, secondValue);
                DrawMetricChip(thirdLabel, thirdValue);
            }
        }

        private static void DrawMetricChip(string label, string value)
        {
            GUILayout.Label($"{label}  {value}", Styles.MetricChip, GUILayout.Height(20f));
        }

        private static void DrawPreview(Rect rect, EnemySO enemy)
        {
            Object previewTarget = enemy != null && enemy.enemyPrefab != null
                ? enemy.enemyPrefab
                : enemy;
            Texture texture = previewTarget != null
                ? EditorGUIUtility.ObjectContent(previewTarget, previewTarget.GetType()).image
                : null;

            EditorGUI.DrawRect(rect, Styles.PreviewBackground);
            if (texture != null)
            {
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.LabelField(rect, "?", Styles.CenteredPreviewFallback);
            }
        }

        private void CreateLists()
        {
            if (enemySerializedObject == null)
            {
                equipmentsList = null;
                statOverridesList = null;
                stateDatasList = null;
                return;
            }

            CreateEquipmentsList();
            CreateStatOverridesList();
            CreateStateDatasList();
        }

        private void CreateEquipmentsList()
        {
            equipmentsList = new ReorderableList(enemySerializedObject, equipmentsProperty, true, true, true, true);
            equipmentsList.drawHeaderCallback = rect => DrawCollectionHeader(rect, "Equipments", $"{snapshot?.EquipmentCount ?? equipmentsProperty.arraySize} slots");
            equipmentsList.elementHeight = EditorGUIUtility.singleLineHeight + 8f;
            equipmentsList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = equipmentsProperty.GetArrayElementAtIndex(index);
                SerializedProperty typeProperty = element.FindPropertyRelative("type");
                SerializedProperty itemDataProperty = element.FindPropertyRelative("itemData");

                Rect row = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                float leftWidth = Mathf.Min(120f, row.width * 0.34f);
                Rect leftRect = new Rect(row.x, row.y, leftWidth, row.height);
                Rect rightRect = new Rect(row.x + leftWidth + 6f, row.y, row.width - leftWidth - 6f, row.height);

                EditorGUI.PropertyField(leftRect, typeProperty, GUIContent.none);
                EditorGUI.PropertyField(rightRect, itemDataProperty, GUIContent.none);
            };
        }

        private void CreateStatOverridesList()
        {
            statOverridesList = new ReorderableList(enemySerializedObject, statOverridesProperty, true, true, true, true);
            statOverridesList.drawHeaderCallback = rect =>
            {
                DrawCollectionHeader(rect, "Stat Overrides", $"{snapshot?.EnabledOverrideCount ?? 0} active");
            };
            statOverridesList.elementHeightCallback = index =>
            {
                SerializedProperty element = statOverridesProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, GUIContent.none, true) + 6f;
            };
            statOverridesList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = statOverridesProperty.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height = EditorGUI.GetPropertyHeight(element, GUIContent.none, true);
                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            };
        }

        private void CreateStateDatasList()
        {
            stateDatasList = new ReorderableList(enemySerializedObject, stateDatasProperty, true, true, true, true);
            stateDatasList.drawHeaderCallback = rect => DrawCollectionHeader(rect, "State Datas", $"{snapshot?.StateCount ?? stateDatasProperty.arraySize} entries");
            stateDatasList.elementHeight = EditorGUIUtility.singleLineHeight + 8f;
            stateDatasList.drawElementCallback = (rect, index, active, focused) =>
            {
                SerializedProperty element = stateDatasProperty.GetArrayElementAtIndex(index);
                Rect row = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(row, element, GUIContent.none);
            };
        }

        private static void DrawCollectionHeader(Rect rect, string title, string summary)
        {
            EditorGUI.LabelField(rect, title, Styles.CollectionTitle);

            Rect summaryRect = new Rect(rect.xMax - 120f, rect.y, 120f, rect.height);
            EditorGUI.LabelField(summaryRect, summary, Styles.CollectionSummary);
        }

        private void HandleLoadStatsFromPrefab()
        {
            ApplyPendingChanges();

            if (LoadStatsFromPrefabMethod == null)
            {
                SetActionMessage("Could not find LoadStatsFromPrefab().", MessageType.Error);
                return;
            }

            Undo.RecordObject(enemyTarget, "Load Enemy Stats From Prefab");
            LoadStatsFromPrefabMethod.Invoke(enemyTarget, null);
            EditorUtility.SetDirty(enemyTarget);

            enemySerializedObject.UpdateIfRequiredOrScript();
            snapshotDirty = true;
            SetActionMessage("Reloaded stat overrides from the prefab source.", MessageType.Info);
        }

        private void HandleSyncAllPatches()
        {
            ApplyPendingChanges();

            Undo.RecordObject(enemyTarget, "Sync EnemySO Patches");
            int patchCount = ForEachPatch(enemyTarget, patch =>
            {
                patch.SyncInputs();
                return true;
            });
            EditorUtility.SetDirty(enemyTarget);

            enemySerializedObject.UpdateIfRequiredOrScript();
            snapshotDirty = true;
            SetActionMessage($"Synced {patchCount} patches.", MessageType.Info);
        }

        private void HandleGenerateSetters()
        {
            ApplyPendingChanges();

            Undo.RecordObject(enemyTarget, "Generate EnemySO Patch Setters");
            int patchCount = ForEachPatch(enemyTarget, patch =>
            {
                patch.GenerateSetter();
                return true;
            });
            EditorUtility.SetDirty(enemyTarget);

            enemySerializedObject.UpdateIfRequiredOrScript();
            snapshotDirty = true;
            SetActionMessage($"Regenerated {patchCount} patch setters.", MessageType.Info);
        }

        private static int ForEachPatch(EnemySO enemy, Func<IFieldPatchRuntime, bool> callback)
        {
            if (enemy == null || callback == null)
            {
                return 0;
            }

            int count = 0;

            if (enemy.behaviourPrefabs != null)
            {
                for (int i = 0; i < enemy.behaviourPrefabs.Length; i++)
                {
                    IFieldPatchRuntime patch = enemy.behaviourPrefabs[i];
                    if (patch != null && callback(patch))
                    {
                        count++;
                    }
                }
            }

            if (enemy.passiveSkill != null)
            {
                foreach (IFieldPatchRuntime patch in enemy.passiveSkill.Values)
                {
                    if (patch != null && callback(patch))
                    {
                        count++;
                    }
                }
            }

            if (enemy.activeSkill != null)
            {
                foreach (IFieldPatchRuntime patch in enemy.activeSkill.Values)
                {
                    if (patch != null && callback(patch))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void ApplyPendingChanges()
        {
            if (enemySerializedObject == null || !enemySerializedObject.hasModifiedProperties)
            {
                return;
            }

            enemySerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(enemyTarget);
            snapshotDirty = true;
        }

        private void SetActionMessage(string message, MessageType messageType)
        {
            actionMessage = message;
            actionMessageType = messageType;
        }

        private void SetAllSectionsExpanded(bool expanded)
        {
            showCore = expanded;
            showEquipment = expanded;
            showStats = expanded;
            showStates = expanded;
            showBehaviours = expanded;
            showSkills = expanded;
            Repaint();
        }

        private string GetCoreSummary()
        {
            return $"{snapshot.PrefabName} / {snapshot.BulletName}";
        }

        private string GetEquipmentSummary()
        {
            return snapshot.MissingEquipmentCount > 0
                ? $"{snapshot.EquipmentCount} slots, {snapshot.MissingEquipmentCount} empty"
                : $"{snapshot.EquipmentCount} slots";
        }

        private string GetStatsSummary()
        {
            return snapshot.DuplicatedStatNames.Count > 0
                ? $"{snapshot.StatCount} entries, duplicates"
                : $"{snapshot.EnabledOverrideCount} active";
        }

        private string GetStatesSummary()
        {
            return snapshot.MissingStateCount > 0
                ? $"{snapshot.StateCount} entries, {snapshot.MissingStateCount} missing"
                : $"{snapshot.StateCount} entries";
        }

        private string GetBehavioursSummary()
        {
            return $"{snapshot.ConfiguredBehaviourCount} / {snapshot.BehaviourCount} configured";
        }

        private string GetSkillsSummary()
        {
            return $"Passive {snapshot.PassiveSkillCount} / Active {snapshot.ActiveSkillCount}";
        }

        private int CountConfiguredBehaviourTargets()
        {
            int count = 0;
            for (int i = 0; i < behaviourPrefabsProperty.arraySize; i++)
            {
                SerializedProperty element = behaviourPrefabsProperty.GetArrayElementAtIndex(i);
                SerializedProperty targetProperty = element.FindPropertyRelative("_target");
                if (targetProperty?.objectReferenceValue != null)
                {
                    count++;
                }
            }

            return count;
        }

        private EditorSnapshot BuildSnapshot(EnemySO enemy)
        {
            EditorSnapshot nextSnapshot = new EditorSnapshot
            {
                EquipmentCount = equipmentsProperty?.arraySize ?? 0,
                StatCount = statOverridesProperty?.arraySize ?? 0,
                StateCount = stateDatasProperty?.arraySize ?? 0,
                BehaviourCount = behaviourPrefabsProperty?.arraySize ?? 0,
                PrefabName = enemy != null && enemy.enemyPrefab != null ? enemy.enemyPrefab.name : "No Prefab",
                BulletName = bulletDataProperty?.objectReferenceValue != null ? bulletDataProperty.objectReferenceValue.name : "No Bullet"
            };

            if (enemy == null)
            {
                nextSnapshot.HeroSubtitle = "Enemy Scriptable Object";
                nextSnapshot.HeroStatusLine = "Configure enemy data, behaviours, and skills from one place.";
                nextSnapshot.Warnings.Add("EnemySO target could not be resolved.");
                return nextSnapshot;
            }

            nextSnapshot.PassiveSkillCount = CountConfiguredSkillPatches(enemy.passiveSkill?.Values);
            nextSnapshot.ActiveSkillCount = CountConfiguredSkillPatches(enemy.activeSkill?.Values);
            nextSnapshot.ConfiguredBehaviourCount = CountConfiguredBehaviourTargets();
            nextSnapshot.HeroSubtitle = enemy.enemyPrefab == null ? "Enemy prefab is not assigned yet" : enemy.enemyPrefab.name;

            if (enemy.enemyPrefab == null)
            {
                nextSnapshot.Warnings.Add("Enemy Prefab is missing.");
            }
            else
            {
                bool hasStatOverrideBehavior = enemy.enemyPrefab.GetComponentInChildren<StatOverrideBehavior>() != null;
                if (!hasStatOverrideBehavior)
                {
                    nextSnapshot.Warnings.Add("Enemy Prefab has no StatOverrideBehavior. Load From Prefab may return nothing.");
                }
            }

            Dictionary<string, int> statOccurrences = new Dictionary<string, int>();

            for (int i = 0; i < nextSnapshot.EquipmentCount; i++)
            {
                SerializedProperty element = equipmentsProperty.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("itemData")?.objectReferenceValue == null)
                {
                    nextSnapshot.MissingEquipmentCount++;
                }
            }

            for (int i = 0; i < nextSnapshot.StatCount; i++)
            {
                SerializedProperty element = statOverridesProperty.GetArrayElementAtIndex(i);

                if (element.FindPropertyRelative("isUseOverride")?.boolValue == true)
                {
                    nextSnapshot.EnabledOverrideCount++;
                }

                Object statObject = element.FindPropertyRelative("stat")?.objectReferenceValue;
                if (statObject == null)
                {
                    nextSnapshot.MissingStatReferenceCount++;
                    continue;
                }

                string statName = GetStatDisplayName(statObject);
                if (statOccurrences.ContainsKey(statName))
                {
                    statOccurrences[statName]++;
                }
                else
                {
                    statOccurrences.Add(statName, 1);
                }
            }

            for (int i = 0; i < nextSnapshot.StateCount; i++)
            {
                if (stateDatasProperty.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    nextSnapshot.MissingStateCount++;
                }
            }

            nextSnapshot.DuplicatedStatNames = statOccurrences
                .Where(pair => pair.Value > 1)
                .Select(pair => pair.Key)
                .OrderBy(name => name)
                .ToList();

            if (nextSnapshot.MissingEquipmentCount > 0)
            {
                nextSnapshot.Warnings.Add($"{nextSnapshot.MissingEquipmentCount} equipment slots are missing Item Data.");
            }

            if (nextSnapshot.MissingStatReferenceCount > 0)
            {
                nextSnapshot.Warnings.Add($"{nextSnapshot.MissingStatReferenceCount} stat overrides are missing Stat references.");
            }

            if (nextSnapshot.DuplicatedStatNames.Count > 0)
            {
                nextSnapshot.Warnings.Add($"Duplicated stat overrides: {string.Join(", ", nextSnapshot.DuplicatedStatNames)}");
            }

            if (nextSnapshot.MissingStateCount > 0)
            {
                nextSnapshot.Warnings.Add($"{nextSnapshot.MissingStateCount} state data entries are missing references.");
            }

            int unconfiguredBehaviours = nextSnapshot.BehaviourCount - nextSnapshot.ConfiguredBehaviourCount;
            if (unconfiguredBehaviours > 0)
            {
                nextSnapshot.Warnings.Add($"{unconfiguredBehaviours} behaviour patches are missing targets.");
            }

            nextSnapshot.HeroStatusLine =
                $"Stats {nextSnapshot.EnabledOverrideCount} active, behaviours {nextSnapshot.ConfiguredBehaviourCount}, skills {nextSnapshot.PassiveSkillCount + nextSnapshot.ActiveSkillCount} configured";

            return nextSnapshot;
        }

        private static int CountConfiguredSkillPatches(IEnumerable<IFieldPatchRuntime> patches)
        {
            if (patches == null)
            {
                return 0;
            }

            int count = 0;
            foreach (IFieldPatchRuntime patch in patches)
            {
                if (patch?.TargetObject != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static string GetStatDisplayName(Object statObject)
        {
            if (statObject == null)
            {
                return "(None)";
            }

            SerializedObject statSerializedObject = new SerializedObject(statObject);
            SerializedProperty statNameProperty = statSerializedObject.FindProperty("statName");
            string statName = statNameProperty?.stringValue;
            return string.IsNullOrWhiteSpace(statName) ? statObject.name : statName;
        }

        private sealed class EditorSnapshot
        {
            public int EquipmentCount;
            public int StatCount;
            public int StateCount;
            public int BehaviourCount;
            public int EnabledOverrideCount;
            public int MissingEquipmentCount;
            public int MissingStatReferenceCount;
            public int MissingStateCount;
            public int ConfiguredBehaviourCount;
            public int PassiveSkillCount;
            public int ActiveSkillCount;
            public string PrefabName = "No Prefab";
            public string BulletName = "No Bullet";
            public string HeroSubtitle = "Enemy Scriptable Object";
            public string HeroStatusLine = string.Empty;
            public List<string> DuplicatedStatNames = new List<string>();
            public List<string> Warnings = new List<string>();
        }

        private static class Styles
        {
            private static GUIStyle selectionCard;
            private static GUIStyle heroCard;
            private static GUIStyle toolbarCard;
            private static GUIStyle sectionCard;
            private static GUIStyle subSectionCard;
            private static GUIStyle heroTitle;
            private static GUIStyle heroSubtitle;
            private static GUIStyle heroStatus;
            private static GUIStyle sideLabel;
            private static GUIStyle metricChip;
            private static GUIStyle toolbarTitle;
            private static GUIStyle sectionFoldout;
            private static GUIStyle sectionSummary;
            private static GUIStyle subSectionTitle;
            private static GUIStyle subSectionSummary;
            private static GUIStyle collectionTitle;
            private static GUIStyle collectionSummary;
            private static GUIStyle contextLabel;
            private static GUIStyle selectionPath;
            private static GUIStyle centeredPreviewFallback;

            public static Color SectionHeaderBackground =>
                EditorGUIUtility.isProSkin ? new Color(0.16f, 0.18f, 0.21f) : new Color(0.85f, 0.88f, 0.91f);

            public static Color PreviewBackground =>
                EditorGUIUtility.isProSkin ? new Color(0.12f, 0.13f, 0.15f) : new Color(0.92f, 0.93f, 0.95f);

            public static GUIStyle SelectionCard => selectionCard ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(4, 4, 4, 6)
            };

            public static GUIStyle HeroCard => heroCard ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(12, 12, 12, 12)
            };

            public static GUIStyle ToolbarCard => toolbarCard ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            public static GUIStyle SectionCard => sectionCard ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            public static GUIStyle SubSectionCard => subSectionCard ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8)
            };

            public static GUIStyle HeroTitle => heroTitle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15
            };

            public static GUIStyle HeroSubtitle => heroSubtitle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                wordWrap = true
            };

            public static GUIStyle HeroStatus => heroStatus ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true
            };

            public static GUIStyle SideLabel => sideLabel ??= new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.UpperLeft
            };

            public static GUIStyle MetricChip => metricChip ??= new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                margin = new RectOffset(0, 6, 0, 0),
                padding = new RectOffset(8, 8, 3, 3)
            };

            public static GUIStyle ToolbarTitle => toolbarTitle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                margin = new RectOffset(0, 0, 0, 6)
            };

            public static GUIStyle SectionFoldout => sectionFoldout ??= new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            public static GUIStyle SectionSummary => sectionSummary ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

            public static GUIStyle SubSectionTitle => subSectionTitle ??= new GUIStyle(EditorStyles.boldLabel);

            public static GUIStyle SubSectionSummary => subSectionSummary ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

            public static GUIStyle CollectionTitle => collectionTitle ??= new GUIStyle(EditorStyles.boldLabel);

            public static GUIStyle CollectionSummary => collectionSummary ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };

            public static GUIStyle ContextLabel => contextLabel ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true
            };

            public static GUIStyle SelectionPath => selectionPath ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true
            };

            public static GUIStyle CenteredPreviewFallback => centeredPreviewFallback ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
