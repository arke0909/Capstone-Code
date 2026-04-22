using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Chipmunk.Modules.StatSystem.Editor
{
    [CustomPropertyDrawer(typeof(StatOverride))]
    public class StatOverrideDrawer : PropertyDrawer
    {
        [SerializeField] private Material statIconMaterial;
        private const float ToggleWidth = 92f;
        private const float ValueLabelWidth = 38f;
        private const float Padding = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            return (line * 2f) + spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty statProperty = property.FindPropertyRelative("stat");
            SerializedProperty useOverrideProperty = property.FindPropertyRelative("isUseOverride");
            SerializedProperty overrideValueProperty = property.FindPropertyRelative("overrideValue");

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginProperty(position, label, property);

            Rect contentRect = EditorGUI.IndentedRect(position);
            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect firstLineRect = new Rect(contentRect.x, contentRect.y, contentRect.width, line);
            Rect secondLineRect = new Rect(contentRect.x, contentRect.y + line + spacing, contentRect.width, line);
            Rect statRect = contentRect;
            statRect.y = firstLineRect.y;
            statRect.height = line;
            statRect.width -= line + Padding;

            Rect iconRect = new Rect(statRect.xMax + Padding, firstLineRect.y, line, line);

            Rect toggleRect = new Rect(secondLineRect.x, secondLineRect.y, ToggleWidth, secondLineRect.height);
            Rect valueLabelRect = new Rect(toggleRect.xMax + Padding, secondLineRect.y, ValueLabelWidth, secondLineRect.height);
            Rect valueRect = new Rect(
                valueLabelRect.xMax + Padding,
                secondLineRect.y,
                Mathf.Max(0f, secondLineRect.xMax - (valueLabelRect.xMax + Padding)),
                secondLineRect.height);

            EditorGUI.BeginChangeCheck();

            Object nextStat = EditorGUI.ObjectField(statRect, statProperty.objectReferenceValue, typeof(StatSO), false);
            bool nextUseOverride = EditorGUI.ToggleLeft(toggleRect, "Override", useOverrideProperty.boolValue);
            EditorGUI.LabelField(valueLabelRect, "Value");
            using (new EditorGUI.DisabledScope(!nextUseOverride))
            {
                float nextOverrideValue = EditorGUI.DelayedFloatField(valueRect, GUIContent.none, overrideValueProperty.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    statProperty.objectReferenceValue = nextStat;
                    useOverrideProperty.boolValue = nextUseOverride;
                    overrideValueProperty.floatValue = nextOverrideValue;
                    ApplyStatOverrideChanges(property.serializedObject);
                }
            }

            if (Event.current.type == EventType.Repaint)
            {
                DrawIcon(iconRect, statProperty.objectReferenceValue);
            }

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }

        private static void ApplyStatOverrideChanges(SerializedObject serializedObject)
        {
            if (serializedObject == null)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();

            if (serializedObject.targetObject != null)
            {
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        private void DrawIcon(Rect iconRect, Object statObject)
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
