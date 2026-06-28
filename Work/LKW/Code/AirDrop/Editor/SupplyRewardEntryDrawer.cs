using Code.AirDrop;
using Code.Items.ItemInfo;
using UnityEditor;
using UnityEngine;

namespace Code.AirDrop.Editor
{
    [CustomPropertyDrawer(typeof(SupplyRewardEntry))]
    public class SupplyRewardEntryDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float Spacing = 4f;
        private const float IconSize = 20f;
        private const float Indent = 16f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return IconSize + Spacing;

            return IconSize
                   + Spacing
                   + LineHeight
                   + LineHeight * 2
                   + Spacing
                   + LineHeight
                   + LineHeight * 4
                   + Spacing
                   + LineHeight
                   + LineHeight * 2
                   + Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty itemData = property.FindPropertyRelative("itemData");
            SerializedProperty minDay = property.FindPropertyRelative("minDay");
            SerializedProperty maxDay = property.FindPropertyRelative("maxDay");
            SerializedProperty guaranteed = property.FindPropertyRelative("guaranteed");
            SerializedProperty weight = property.FindPropertyRelative("weight");
            SerializedProperty cost = property.FindPropertyRelative("cost");
            SerializedProperty maxPickCount = property.FindPropertyRelative("maxPickCount");
            SerializedProperty minStack = property.FindPropertyRelative("minStack");
            SerializedProperty maxStack = property.FindPropertyRelative("maxStack");

            Rect headerRect = new Rect(position.x, position.y, position.width, IconSize);
            DrawHeader(headerRect, property, itemData);

            if (property.isExpanded)
            {
                float y = headerRect.yMax + Spacing;
                Rect bodyRect = new Rect(position.x + Indent, y, position.width - Indent, position.height - IconSize - Spacing);

                DrawSectionLabel(ref bodyRect, "Availability");
                DrawProperty(ref bodyRect, minDay, "Min Day");
                DrawProperty(ref bodyRect, maxDay, "Max Day");

                bodyRect.y += Spacing;
                DrawSectionLabel(ref bodyRect, "Pick Rule");
                DrawProperty(ref bodyRect, guaranteed, "Guaranteed");
                DrawProperty(ref bodyRect, weight, "Weight");
                DrawProperty(ref bodyRect, cost, "Cost");
                DrawProperty(ref bodyRect, maxPickCount, "Max Pick Count");

                bodyRect.y += Spacing;
                DrawSectionLabel(ref bodyRect, "Stack");
                DrawProperty(ref bodyRect, minStack, "Min Stack");
                DrawProperty(ref bodyRect, maxStack, "Max Stack");
            }

            EditorGUI.EndProperty();
        }

        private static void DrawHeader(Rect rect, SerializedProperty property, SerializedProperty itemData)
        {
            Rect foldoutRect = new Rect(rect.x, rect.y, 14f, rect.height);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

            Rect iconRect = new Rect(rect.x + 16f, rect.y, IconSize, IconSize);
            DrawItemIcon(iconRect, itemData.objectReferenceValue as ItemDataSO);

            Rect fieldRect = new Rect(iconRect.xMax + Spacing, rect.y, rect.width - iconRect.width - 20f, rect.height);
            EditorGUI.PropertyField(fieldRect, itemData, GUIContent.none);
        }

        private static void DrawItemIcon(Rect rect, ItemDataSO itemData)
        {
            Texture icon = null;

            if (itemData != null)
            {
                if (itemData.itemImage != null)
                    icon = AssetPreview.GetAssetPreview(itemData.itemImage) ?? AssetPreview.GetMiniThumbnail(itemData.itemImage);

                icon ??= AssetPreview.GetMiniThumbnail(itemData);
            }

            if (icon != null)
                GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
            else
                EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));
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
