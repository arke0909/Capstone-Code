using Code.AirDrop;
using UnityEditor;
using UnityEngine;

namespace Code.AirDrop.Editor
{
    [CustomPropertyDrawer(typeof(SupplyGuaranteedGroup))]
    public class SupplyGuaranteedGroupDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float Spacing = 4f;
        private const float SectionSpacing = 6f;
        private const float Indent = 16f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return LineHeight + Spacing;

            SerializedProperty rewardEntries = property.FindPropertyRelative("rewardEntries");
            return LineHeight
                   + SectionSpacing
                   + LineHeight
                   + LineHeight * 2
                   + SectionSpacing
                   + LineHeight
                   + LineHeight
                   + SectionSpacing
                   + EditorGUI.GetPropertyHeight(rewardEntries, true)
                   + Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty groupName = property.FindPropertyRelative("groupName");
            SerializedProperty minDay = property.FindPropertyRelative("minDay");
            SerializedProperty maxDay = property.FindPropertyRelative("maxDay");
            SerializedProperty pickCount = property.FindPropertyRelative("pickCount");
            SerializedProperty rewardEntries = property.FindPropertyRelative("rewardEntries");

            Rect headerRect = new Rect(position.x, position.y, position.width, LineHeight);
            DrawHeader(headerRect, property, groupName, rewardEntries);

            if (property.isExpanded)
            {
                float y = headerRect.yMax + SectionSpacing;
                Rect bodyRect = new Rect(position.x + Indent, y, position.width - Indent, position.height - LineHeight - Spacing);

                DrawSectionLabel(ref bodyRect, "Availability");
                DrawProperty(ref bodyRect, minDay, "Min Day");
                DrawProperty(ref bodyRect, maxDay, "Max Day");

                bodyRect.y += SectionSpacing;
                DrawSectionLabel(ref bodyRect, "Pick");
                DrawProperty(ref bodyRect, pickCount, "Pick Count");

                bodyRect.y += SectionSpacing;
                Rect rewardRect = new Rect(
                    bodyRect.x,
                    bodyRect.y,
                    bodyRect.width,
                    EditorGUI.GetPropertyHeight(rewardEntries, true));
                EditorGUI.PropertyField(rewardRect, rewardEntries, true);
            }

            EditorGUI.EndProperty();
        }

        private static void DrawHeader(
            Rect rect,
            SerializedProperty property,
            SerializedProperty groupName,
            SerializedProperty rewardEntries)
        {
            Rect foldoutRect = new Rect(rect.x, rect.y, 14f, rect.height);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

            Rect nameRect = new Rect(rect.x + 16f, rect.y, rect.width - 92f, rect.height);
            EditorGUI.PropertyField(nameRect, groupName, GUIContent.none);

            Rect countRect = new Rect(nameRect.xMax + Spacing, rect.y, 72f, rect.height);
            EditorGUI.LabelField(countRect, $"{rewardEntries.arraySize} entries", EditorStyles.miniLabel);
        }

        private static void DrawSectionLabel(ref Rect bodyRect, string text)
        {
            Rect rect = NextLine(ref bodyRect);
            EditorGUI.LabelField(rect, text, EditorStyles.boldLabel);
        }

        private static void DrawProperty(ref Rect bodyRect, SerializedProperty property, string label)
        {
            Rect rect = NextLine(ref bodyRect);
            EditorGUI.PropertyField(rect, property, new GUIContent(label));
        }

        private static Rect NextLine(ref Rect bodyRect)
        {
            Rect rect = new Rect(bodyRect.x, bodyRect.y, bodyRect.width, LineHeight);
            bodyRect.y += LineHeight;
            return rect;
        }
    }
}
