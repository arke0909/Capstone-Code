using Scripts.SkillSystem.Skills;
using Scripts.SkillSystem;
using UnityEditor;
using UnityEngine;

namespace Code.EnemySpawn.Editor
{
    // [CustomPropertyDrawer(typeof(EnemySkillConfig))]
    public class EnemySkillConfigDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = lineHeight;

            if (!property.isExpanded)
                return height;

            SerializedProperty skillPrefabProperty = property.FindPropertyRelative("skillPrefab");
            SerializedProperty targetStateProperty = property.FindPropertyRelative("targetState");
            SerializedProperty behaviorSettingProperty = property.FindPropertyRelative("behaviorSetting");

            height += spacing + EditorGUI.GetPropertyHeight(skillPrefabProperty, true);

            if (ShouldDisplayTargetState(skillPrefabProperty))
                height += spacing + EditorGUI.GetPropertyHeight(targetStateProperty, true);

            height += spacing + EditorGUI.GetPropertyHeight(behaviorSettingProperty, true);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            Rect rowRect = new Rect(position.x, position.y, position.width, lineHeight);

            property.isExpanded = EditorGUI.Foldout(rowRect, property.isExpanded, label, true);
            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            SerializedProperty skillPrefabProperty = property.FindPropertyRelative("skillPrefab");
            SerializedProperty targetStateProperty = property.FindPropertyRelative("targetState");
            SerializedProperty behaviorSettingProperty = property.FindPropertyRelative("behaviorSetting");

            rowRect.y += lineHeight + spacing;
            rowRect.height = EditorGUI.GetPropertyHeight(skillPrefabProperty, true);
            EditorGUI.PropertyField(rowRect, skillPrefabProperty);

            if (ShouldDisplayTargetState(skillPrefabProperty))
            {
                rowRect.y += rowRect.height + spacing;
                rowRect.height = EditorGUI.GetPropertyHeight(targetStateProperty, true);
                EditorGUI.PropertyField(rowRect, targetStateProperty, new GUIContent("Target State"));
            }

            rowRect.y += rowRect.height + spacing;
            rowRect.height = EditorGUI.GetPropertyHeight(behaviorSettingProperty, true);
            EditorGUI.PropertyField(rowRect, behaviorSettingProperty, true);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private static bool ShouldDisplayTargetState(SerializedProperty skillPrefabProperty)
        {
            GameObject skillPrefab = skillPrefabProperty?.objectReferenceValue as GameObject;
            if (skillPrefab == null)
                return false;

            ActiveSkill skill = skillPrefab.GetComponent<ActiveSkill>();
            return skill is IUseStateSkill;
        }
    }
}

