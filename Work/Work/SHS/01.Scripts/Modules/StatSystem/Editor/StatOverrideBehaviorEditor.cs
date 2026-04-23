using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Chipmunk.Modules.StatSystem.Editor
{
    [CustomEditor(typeof(StatOverrideBehavior))]
    [CanEditMultipleObjects]
    public class StatOverrideBehaviorEditor : UnityEditor.Editor
    {
        [SerializeField] private Material statIconMaterial;
        private const float PreviewIconSize = 24f;

        private readonly HashSet<string> duplicatedStatNames = new();

        private SerializedProperty statOverridesProperty;

        private void OnEnable()
        {
            statOverridesProperty = serializedObject.FindProperty("statOverrides");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScriptField();
            DrawOverviewSection();

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(statOverridesProperty, includeChildren: true);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptField()
        {
            if (target is not MonoBehaviour targetBehavior)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                MonoScript script = MonoScript.FromMonoBehaviour(targetBehavior);
                EditorGUILayout.ObjectField("Script", script, typeof(MonoScript), false);
            }
        }

        private void DrawOverviewSection()
        {
            EditorGUILayout.LabelField("Stat Overview", EditorStyles.boldLabel);

            if (statOverridesProperty == null)
            {
                EditorGUILayout.HelpBox("Failed to find statOverrides property.", MessageType.Error);
                return;
            }

            if (statOverridesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No stat overrides configured.", MessageType.Info);
                return;
            }

            bool hasMissingStatReference = false;
            duplicatedStatNames.Clear();
            HashSet<string> uniqueStatNames = new();

            for (int index = 0; index < statOverridesProperty.arraySize; index++)
            {
                SerializedProperty element = statOverridesProperty.GetArrayElementAtIndex(index);
                Object statObject = GetStatObject(element);

                if (statObject == null)
                {
                    hasMissingStatReference = true;
                    continue;
                }

                string statName = GetStatDisplayName(statObject);
                if (!uniqueStatNames.Add(statName))
                {
                    duplicatedStatNames.Add(statName);
                }
            }

            if (hasMissingStatReference)
            {
                EditorGUILayout.HelpBox("Some recipes are missing Stat references.", MessageType.Warning);
            }

            if (duplicatedStatNames.Count > 0)
            {
                string duplicateList = string.Join(", ", duplicatedStatNames);
                EditorGUILayout.HelpBox($"Duplicated stat names: {duplicateList}", MessageType.Warning);
            }

            for (int index = 0; index < statOverridesProperty.arraySize; index++)
            {
                DrawOverviewRow(index);
            }
        }

        private void DrawOverviewRow(int index)
        {
            SerializedProperty element = statOverridesProperty.GetArrayElementAtIndex(index);
            Object statObject = GetStatObject(element);
            bool useOverride = element.FindPropertyRelative("isUseOverride")?.boolValue ?? false;
            float overrideValue = element.FindPropertyRelative("overrideValue")?.floatValue ?? 0f;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                Rect iconRect = GUILayoutUtility.GetRect(
                    PreviewIconSize,
                    PreviewIconSize,
                    GUILayout.Width(PreviewIconSize),
                    GUILayout.Height(PreviewIconSize));

                DrawOverviewIcon(iconRect, statObject);

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(GetStatDisplayName(statObject), EditorStyles.boldLabel);
                    string stateLabel = useOverride
                        ? $"Override Value: {overrideValue:0.###}"
                        : "Using Stat Base Value";
                    EditorGUILayout.LabelField(stateLabel, EditorStyles.miniLabel);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label(useOverride ? "OVERRIDE" : "BASE", EditorStyles.miniBoldLabel, GUILayout.Width(70f));
            }
        }

        private static Object GetStatObject(SerializedProperty element)
        {
            return element.FindPropertyRelative("stat")?.objectReferenceValue;
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

        private void DrawOverviewIcon(Rect iconRect, Object statObject)
        {
            Texture texture = TryGetStatIconTexture(statObject);
            if (texture == null)
            {
                EditorGUI.DrawRect(iconRect, new Color(0f, 0f, 0f, 0.08f));
                EditorGUI.LabelField(iconRect, "-", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            EditorGUI.DrawPreviewTexture(iconRect, texture, statIconMaterial, ScaleMode.ScaleToFit);
        }

        private static Texture TryGetStatIconTexture(Object statObject)
        {
            if (statObject == null)
            {
                return null;
            }

            SerializedObject statSerializedObject = new SerializedObject(statObject);
            SerializedProperty iconProperty = statSerializedObject.FindProperty("icon");
            Sprite iconSprite = iconProperty?.objectReferenceValue as Sprite;
            return iconSprite != null ? iconSprite.texture : null;
        }
    }
}