using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Code.SHS.Utility.DynamicFieldBinding;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Code.SHS.Utility.DynamicFieldBinding.Editor
{
    [CustomPropertyDrawer(typeof(FieldPatch<>), true)]
        public class FieldPatchDrawer : PropertyDrawer
        {
            private const float HelpBoxLines = 2.2f;
            private static readonly Dictionary<string, string> MessageByKey = new Dictionary<string, string>();
            private static readonly Dictionary<string, bool> ManagedReferenceFoldoutStateByKey =
                new Dictionary<string, bool>();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = line;

            SerializedProperty targetProperty = property.FindPropertyRelative("_target");
            bool hasTarget = HasTarget(targetProperty);

            if (!hasTarget || !property.isExpanded)
            {
                return height;
            }

            height += spacing + line; // buttons

            IFieldPatchRuntime binder = SerializedPropertyRuntime.GetValue(property) as IFieldPatchRuntime;
            if (binder == null || binder.TargetObject == null)
            {
                height += spacing + GetHelpBoxHeight();
            }
            else
            {
                SerializedProperty inputsProperty = property.FindPropertyRelative("_inputs");
                IReadOnlyList<FieldInfo> fields = binder.GetMutableFields();
                for (int i = 0; i < fields.Count; i++)
                {
                    FieldInfo field = fields[i];
                    string fieldKey = FieldPatchUtility.GetFieldKey(field);
                    SerializedProperty inputProperty = FindInputByKey(inputsProperty, fieldKey);

                    height += spacing + line; // field header
                    height += spacing + line; // current
                    height += spacing + GetInputBlockHeight(inputProperty, field, binder.TargetObject); // input

                    if (FieldPatchUtility.GetValueKind(field) == FieldPatchValueKind.Unsupported)
                    {
                        height += spacing + GetHelpBoxHeight();
                    }
                }
            }

            if (TryGetMessage(property, out string message) && !string.IsNullOrEmpty(message))
            {
                height += spacing + GetHelpBoxHeight();
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.BeginProperty(position, label, property);

            Rect row = new Rect(position.x, position.y, position.width, line);
            SerializedProperty targetProperty = property.FindPropertyRelative("_target");
            bool hasTarget = HasTarget(targetProperty);
            if (!hasTarget)
            {
                property.isExpanded = false;
            }

            Rect targetFieldRect = DrawHeaderAndTargetField(row, property, label, hasTarget);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(targetFieldRect, targetProperty, GUIContent.none, true);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                SyncBinder(property, "Change Field Patch Target");
                property.serializedObject.Update();

                hasTarget = HasTarget(targetProperty);
                if (!hasTarget)
                {
                    property.isExpanded = false;
                    SetMessage(property, "Target cleared.");
                }
                else
                {
                    SetMessage(property, "Target changed and field layout synced.");
                }
            }

            if (!hasTarget || !property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            row.y += line + spacing;
            DrawButtons(row, property);

            row.y += line + spacing;
            IFieldPatchRuntime binder = SerializedPropertyRuntime.GetValue(property) as IFieldPatchRuntime;

            if (binder == null || binder.TargetObject == null)
            {
                DrawHelpBox(ref row, "Drag and drop target object to display mutable fields.", MessageType.Info);
            }
            else
            {
                TryAutoSync(property, binder);
                property.serializedObject.Update();
                DrawFieldEditors(ref row, property, binder);
            }

            if (TryGetMessage(property, out string message) && !string.IsNullOrEmpty(message))
            {
                DrawHelpBox(ref row, message, MessageType.None);
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private static bool HasTarget(SerializedProperty targetProperty)
        {
            return targetProperty != null && targetProperty.objectReferenceValue != null;
        }

        private static Rect DrawHeaderAndTargetField(Rect row, SerializedProperty property, GUIContent label, bool hasTarget)
        {
            const float foldoutWidth = 14f;

            if (hasTarget)
            {
                Rect foldoutRect = new Rect(row.x + (EditorGUI.indentLevel * 15f), row.y, foldoutWidth, row.height);
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, false);
            }

            int labelId = GUIUtility.GetControlID(FocusType.Passive);
            Rect targetFieldRect = EditorGUI.PrefixLabel(row, labelId, label);
            if (hasTarget)
            {
                targetFieldRect.xMin += foldoutWidth;
            }

            return targetFieldRect;
        }

        private static void DrawButtons(Rect row, SerializedProperty property)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float width = (row.width - (spacing * 2f)) / 3f;

            Rect syncRect = new Rect(row.x, row.y, width, row.height);
            Rect generateRect = new Rect(syncRect.xMax + spacing, row.y, width, row.height);
            Rect applyRect = new Rect(generateRect.xMax + spacing, row.y, width, row.height);

            if (GUI.Button(syncRect, "Sync"))
            {
                property.serializedObject.ApplyModifiedProperties();
                SyncBinder(property, "Sync Patch Inputs");
                property.serializedObject.Update();
                SetMessage(property, "Input values synced from target fields.");
            }

            if (GUI.Button(generateRect, "Generate Setter"))
            {
                property.serializedObject.ApplyModifiedProperties();

                IFieldPatchRuntime binder = SerializedPropertyRuntime.GetValue(property) as IFieldPatchRuntime;
                if (binder == null)
                {
                    SetMessage(property, "Field patch is not available.");
                }
                else
                {
                    UnityEngine.Object host = property.serializedObject.targetObject;
                    Undo.RecordObject(host, "Generate Setter");

                    try
                    {
                        binder.GenerateSetter();
                        SetMessage(property, "Setter generated successfully.");
                    }
                    catch (Exception ex)
                    {
                        SetMessage(property, ex.Message);
                    }

                    EditorUtility.SetDirty(host);
                }

                property.serializedObject.Update();
            }

            if (GUI.Button(applyRect, "Apply Setter"))
            {
                property.serializedObject.ApplyModifiedProperties();

                IFieldPatchRuntime binder = SerializedPropertyRuntime.GetValue(property) as IFieldPatchRuntime;
                if (binder == null)
                {
                    SetMessage(property, "Field patch is not available.");
                }
                else
                {
                    UnityEngine.Object host = property.serializedObject.targetObject;
                    Undo.RecordObject(host, "Apply Setter");

                    if (binder.TargetObject != null)
                    {
                        Undo.RecordObject(binder.TargetObject, "Apply Setter");
                    }

                    try
                    {
                        binder.ApplySetter();
                        SetMessage(property, "Setter applied successfully.");
                    }
                    catch (Exception ex)
                    {
                        SetMessage(property, ex.Message);
                    }

                    EditorUtility.SetDirty(host);

                    if (binder.TargetObject != null)
                    {
                        EditorUtility.SetDirty(binder.TargetObject);
                    }
                }

                property.serializedObject.Update();
            }
        }

        private static void DrawFieldEditors(ref Rect row, SerializedProperty property, IFieldPatchRuntime binder)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            UnityEngine.Object targetObject = binder.TargetObject;
            SerializedProperty inputsProperty = property.FindPropertyRelative("_inputs");
            IReadOnlyList<FieldInfo> fields = binder.GetMutableFields();

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo field = fields[i];
                string fieldLabel = ObjectNames.NicifyVariableName(FieldPatchUtility.GetDisplayName(field));

                Rect headerRow = new Rect(row.x, row.y, row.width, line);
                EditorGUI.LabelField(headerRow, $"{fieldLabel} ({field.FieldType.Name})", EditorStyles.boldLabel);
                row.y += line + spacing;

                object currentValue = ReadFieldValue(targetObject, field);

                using (new EditorGUI.DisabledScope(true))
                {
                    Rect currentRow = new Rect(row.x, row.y, row.width, line);
                    DrawReadonlyValue(currentRow, field.FieldType, currentValue, "Current");
                    row.y += line + spacing;
                }

                string fieldKey = FieldPatchUtility.GetFieldKey(field);
                SerializedProperty inputProperty = FindInputByKey(inputsProperty, fieldKey);
                FieldPatchValueKind valueKind = FieldPatchUtility.GetValueKind(field);

                if (inputProperty == null)
                {
                    Rect missingRow = new Rect(row.x, row.y, row.width, line);
                    EditorGUI.HelpBox(missingRow, $"Input entry missing: {field.Name}", MessageType.Warning);
                    row.y += line + spacing;
                    continue;
                }

                SerializedProperty kindProperty = inputProperty.FindPropertyRelative("_valueKind");
                if (kindProperty != null)
                {
                    kindProperty.enumValueIndex = (int)valueKind;
                }

                SerializedProperty overrideProperty = inputProperty.FindPropertyRelative("_hasOverride");
                bool wasOverride = GetOverrideValue(inputProperty, overrideProperty);

                bool useSeparateToggleRow = valueKind == FieldPatchValueKind.ManagedReference
                    || valueKind == FieldPatchValueKind.List;
                const float toggleWidth = 78f;
                float inputHeight = GetInputHeight(inputProperty, field, targetObject);
                float rowHeight = useSeparateToggleRow
                    ? line + spacing + Mathf.Max(line, inputHeight)
                    : Mathf.Max(line, inputHeight);
                Rect inputRow = new Rect(row.x, row.y, row.width, rowHeight);
                Rect toggleRect = useSeparateToggleRow
                    ? new Rect(inputRow.x, inputRow.y, inputRow.width, line)
                    : new Rect(inputRow.x, inputRow.y, toggleWidth, line);
                Rect valueRect = useSeparateToggleRow
                    ? new Rect(
                        inputRow.x,
                        inputRow.y + line + spacing,
                        inputRow.width,
                        inputHeight)
                    : new Rect(
                        toggleRect.xMax + 4f,
                        inputRow.y,
                        Mathf.Max(0f, inputRow.width - toggleWidth - 4f),
                        inputHeight);

                bool nextOverride = EditorGUI.ToggleLeft(toggleRect, "Override", wasOverride);
                if (nextOverride != wasOverride)
                {
                    SetOverrideValue(inputProperty, overrideProperty, nextOverride);
                }

                if (!nextOverride)
                {
                    if (valueKind == FieldPatchValueKind.ManagedReference)
                    {
                        DrawReadonlyManagedReferenceValue(valueRect, currentValue, field, inputProperty);
                    }
                    else if (valueKind == FieldPatchValueKind.List)
                    {
                        DrawReadonlySupportedListValue(valueRect, currentValue, field, inputProperty, "Input");
                    }
                    else
                    {
                        SyncInputValueFromCurrent(inputProperty, currentValue, field, valueKind, targetObject);
                        DrawReadonlyValue(valueRect, field.FieldType, currentValue, "Input");
                    }
                }
                else
                {
                    if (!wasOverride)
                    {
                        SyncInputValueFromCurrent(inputProperty, currentValue, field, valueKind, targetObject);
                    }

                    EditorGUI.BeginChangeCheck();
                    DrawInputValue(valueRect, inputProperty, field, valueKind, targetObject);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ApplyInputPropertyChanges(inputProperty.serializedObject);
                    }
                }

                row.y += rowHeight + spacing;

                if (valueKind == FieldPatchValueKind.Unsupported)
                {
                    DrawHelpBox(ref row, $"Unsupported input type: {field.FieldType.FullName}", MessageType.Warning);
                }
            }
        }

        private static void DrawReadonlyValue(Rect rect, Type fieldType, object value, string label)
        {
            if (fieldType == typeof(int))
            {
                EditorGUI.IntField(rect, label, value is int typed ? typed : default);
                return;
            }

            if (fieldType == typeof(float))
            {
                EditorGUI.FloatField(rect, label, value is float typed ? typed : default);
                return;
            }

            if (fieldType == typeof(double))
            {
                EditorGUI.DoubleField(rect, label, value is double typed ? typed : default);
                return;
            }

            if (fieldType == typeof(long))
            {
                EditorGUI.LongField(rect, label, value is long typed ? typed : default);
                return;
            }

            if (fieldType == typeof(bool))
            {
                EditorGUI.Toggle(rect, label, value is bool typed && typed);
                return;
            }

            if (fieldType == typeof(string))
            {
                EditorGUI.TextField(rect, label, value as string ?? string.Empty);
                return;
            }

            if (fieldType == typeof(Vector2))
            {
                EditorGUI.Vector2Field(rect, label, value is Vector2 typed ? typed : default);
                return;
            }

            if (fieldType == typeof(Vector3))
            {
                EditorGUI.Vector3Field(rect, label, value is Vector3 typed ? typed : default);
                return;
            }

            if (fieldType == typeof(Vector4))
            {
                EditorGUI.Vector4Field(rect, label, value is Vector4 typed ? typed : default);
                return;
            }

            if (fieldType == typeof(Color))
            {
                EditorGUI.ColorField(rect, label, value is Color typed ? typed : Color.white);
                return;
            }

            if (fieldType == typeof(LayerMask))
            {
                DrawLayerMaskField(rect, label, value is LayerMask typed ? typed : default);
                return;
            }

            if (fieldType.IsEnum)
            {
                Enum enumValue = value as Enum;
                if (enumValue == null)
                {
                    Array values = Enum.GetValues(fieldType);
                    if (values.Length > 0)
                    {
                        enumValue = (Enum)values.GetValue(0);
                    }
                }

                if (enumValue != null)
                {
                    EditorGUI.EnumPopup(rect, label, enumValue);
                }
                else
                {
                    EditorGUI.LabelField(rect, label, "N/A");
                }

                return;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                EditorGUI.ObjectField(rect, label, value as UnityEngine.Object, fieldType, true);
                return;
            }

            EditorGUI.LabelField(rect, label, value != null ? value.ToString() : "null");
        }

        private static void DrawInputValue(Rect rect, SerializedProperty inputProperty, FieldInfo field, FieldPatchValueKind valueKind,
            UnityEngine.Object targetObject)
        {
            Type fieldType = field?.FieldType;
            switch (valueKind)
            {
                case FieldPatchValueKind.Int:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_intValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Float:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_floatValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Double:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_doubleValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Long:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_longValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Bool:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_boolValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.String:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_stringValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Vector2:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_vector2Value"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Vector3:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_vector3Value"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Vector4:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_vector4Value"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Color:
                    EditorGUI.PropertyField(rect, inputProperty.FindPropertyRelative("_colorValue"), new GUIContent("Input"));
                    break;
                case FieldPatchValueKind.Enum:
                    DrawEnumInput(rect, inputProperty.FindPropertyRelative("_enumValue"), fieldType);
                    break;
                case FieldPatchValueKind.ObjectReference:
                    DrawObjectInput(rect, inputProperty.FindPropertyRelative("_objectValue"), fieldType);
                    break;
                case FieldPatchValueKind.List:
                    DrawSupportedListInput(rect, inputProperty, field);
                    break;
                case FieldPatchValueKind.ManagedReference:
                    DrawManagedReferenceInput(rect, inputProperty, field, targetObject);
                    break;
                default:
                    EditorGUI.LabelField(rect, "Input", "Not supported");
                    break;
            }
        }

        private static void SyncInputValueFromCurrent(SerializedProperty inputProperty, object currentValue, FieldInfo field,
            FieldPatchValueKind valueKind, UnityEngine.Object targetObject)
        {
            if (inputProperty == null)
            {
                return;
            }

            Type fieldType = field?.FieldType;

            switch (valueKind)
            {
                case FieldPatchValueKind.Int:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_intValue");
                        if (prop != null) prop.intValue = currentValue is int typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Float:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_floatValue");
                        if (prop != null) prop.floatValue = currentValue is float typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Double:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_doubleValue");
                        if (prop != null) prop.doubleValue = currentValue is double typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Long:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_longValue");
                        if (prop != null) prop.longValue = currentValue is long typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Bool:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_boolValue");
                        if (prop != null) prop.boolValue = currentValue is bool typed && typed;
                        break;
                    }
                case FieldPatchValueKind.String:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_stringValue");
                        if (prop != null) prop.stringValue = currentValue as string;
                        break;
                    }
                case FieldPatchValueKind.Vector2:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_vector2Value");
                        if (prop != null) prop.vector2Value = currentValue is Vector2 typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Vector3:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_vector3Value");
                        if (prop != null) prop.vector3Value = currentValue is Vector3 typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Vector4:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_vector4Value");
                        if (prop != null) prop.vector4Value = currentValue is Vector4 typed ? typed : default;
                        break;
                    }
                case FieldPatchValueKind.Color:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_colorValue");
                        if (prop != null) prop.colorValue = currentValue is Color typed ? typed : Color.white;
                        break;
                    }
                case FieldPatchValueKind.Enum:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_enumValue");
                        if (prop != null)
                        {
                            prop.intValue = currentValue != null ? Convert.ToInt32(currentValue) : 0;
                        }

                        break;
                    }
                case FieldPatchValueKind.ObjectReference:
                    {
                        SerializedProperty prop = inputProperty.FindPropertyRelative("_objectValue");
                        if (prop != null)
                        {
                            prop.objectReferenceValue = currentValue as UnityEngine.Object;
                        }

                        break;
                    }
                case FieldPatchValueKind.List:
                    {
                        SetManagedReferenceValue(
                            inputProperty,
                            CloneSupportedListValue(currentValue, fieldType));
                        break;
                    }
                case FieldPatchValueKind.ManagedReference:
                    {
                        SetManagedReferenceValue(
                            inputProperty,
                            CloneManagedReferenceValue(currentValue, targetObject, field));
                        break;
                    }
            }
        }
        private static void DrawEnumInput(Rect rect, SerializedProperty enumValueProperty, Type enumType)
        {
            Array enumValues = Enum.GetValues(enumType);
            string[] enumNames = Enum.GetNames(enumType);

            if (enumValues.Length == 0)
            {
                EditorGUI.LabelField(rect, "Input", "Enum has no values");
                return;
            }

            int storedValue = enumValueProperty.intValue;
            int currentIndex = 0;

            for (int i = 0; i < enumValues.Length; i++)
            {
                int enumInt = Convert.ToInt32(enumValues.GetValue(i));
                if (enumInt == storedValue)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = EditorGUI.Popup(rect, "Input", currentIndex, enumNames);
            enumValueProperty.intValue = Convert.ToInt32(enumValues.GetValue(nextIndex));
        }

        private static void DrawObjectInput(Rect rect, SerializedProperty objectProperty, Type fieldType)
        {
            UnityEngine.Object current = objectProperty.objectReferenceValue;
            UnityEngine.Object next = EditorGUI.ObjectField(rect, "Input", current, fieldType, true);
            objectProperty.objectReferenceValue = next;
        }

        private static void DrawManagedReferenceInput(Rect rect, SerializedProperty inputProperty, FieldInfo field, UnityEngine.Object targetObject)
        {
            if (field == null)
            {
                EditorGUI.LabelField(rect, "Input", "Managed reference field is missing");
                return;
            }

            object currentValue = GetManagedReferenceValue(inputProperty);
            string stateKey = GetManagedReferenceStateKey(inputProperty, field);

            if (TryGetManagedReferenceListElementType(field.FieldType, out Type elementType))
            {
                if (DrawManagedReferenceListInput(rect, currentValue as IList, field.FieldType, elementType, inputProperty, stateKey, out object nextListValue))
                {
                    SetManagedReferenceValue(inputProperty, nextListValue);
                }

                return;
            }

            if (DrawManagedReferenceObjectInput(rect, currentValue, field.FieldType, inputProperty, stateKey, out object nextObjectValue))
            {
                SetManagedReferenceValue(inputProperty, nextObjectValue);
            }
        }

        private static float GetInputHeight(SerializedProperty inputProperty, FieldInfo field, UnityEngine.Object targetObject)
        {
            float line = EditorGUIUtility.singleLineHeight;
            if (field == null)
            {
                return line;
            }

            FieldPatchValueKind valueKind = FieldPatchUtility.GetValueKind(field);
            if ((valueKind != FieldPatchValueKind.ManagedReference && valueKind != FieldPatchValueKind.List)
                || inputProperty == null)
            {
                return line;
            }

            SerializedProperty overrideProperty = inputProperty.FindPropertyRelative("_hasOverride");
            bool hasOverride = GetOverrideValue(inputProperty, overrideProperty);
            if (!hasOverride)
            {
                if (valueKind == FieldPatchValueKind.ManagedReference)
                {
                    object currentValue = ReadFieldValue(targetObject, field);
                    return Mathf.Max(line, GetReadonlyManagedReferenceHeight(currentValue, field, inputProperty));
                }

                if (valueKind == FieldPatchValueKind.List)
                {
                    object currentValue = ReadFieldValue(targetObject, field);
                    return Mathf.Max(line, GetReadonlySupportedListHeight(currentValue, field, inputProperty));
                }

                return line;
            }

            if (targetObject == null)
            {
                return line;
            }

            return valueKind == FieldPatchValueKind.ManagedReference
                ? Mathf.Max(line, GetManagedReferenceInputHeight(inputProperty, field))
                : Mathf.Max(line, GetSupportedListInputHeight(inputProperty, field));
        }

        private static float GetInputBlockHeight(SerializedProperty inputProperty, FieldInfo field, UnityEngine.Object targetObject)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float inputHeight = GetInputHeight(inputProperty, field, targetObject);

            FieldPatchValueKind valueKind = FieldPatchUtility.GetValueKind(field);
            return valueKind == FieldPatchValueKind.ManagedReference || valueKind == FieldPatchValueKind.List
                ? line + spacing + Mathf.Max(line, inputHeight)
                : Mathf.Max(line, inputHeight);
        }

        private static bool GetOverrideValue(SerializedProperty inputProperty, SerializedProperty overrideProperty)
        {
            FieldPatchValue inputValue = SerializedPropertyRuntime.GetValue(inputProperty) as FieldPatchValue;
            if (inputValue != null)
            {
                return inputValue.HasOverride;
            }

            return overrideProperty == null || overrideProperty.boolValue;
        }

        private static void SetOverrideValue(SerializedProperty inputProperty, SerializedProperty overrideProperty, bool hasOverride)
        {
            FieldPatchValue inputValue = SerializedPropertyRuntime.GetValue(inputProperty) as FieldPatchValue;
            UnityEngine.Object hostObject = inputProperty?.serializedObject?.targetObject;

            if (hostObject != null)
            {
                Undo.RecordObject(hostObject, "Toggle Field Patch Override");
            }

            inputValue?.SetOverride(hasOverride);

            if (overrideProperty != null)
            {
                overrideProperty.boolValue = hasOverride;
            }

            if (hostObject != null)
            {
                EditorUtility.SetDirty(hostObject);
            }

            InternalEditorUtility.RepaintAllViews();
        }

        private static void ApplyInputPropertyChanges(SerializedObject serializedObject)
        {
            if (serializedObject == null || !serializedObject.hasModifiedProperties)
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();

            if (serializedObject.targetObject != null)
            {
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        private static void DrawSupportedListInput(Rect rect, SerializedProperty inputProperty, FieldInfo field)
        {
            if (field == null)
            {
                EditorGUI.LabelField(rect, "Input", "List field is missing");
                return;
            }

            if (!FieldPatchUtility.TryGetSupportedListElementType(field.FieldType, out Type elementType))
            {
                EditorGUI.LabelField(rect, "Input", $"Unsupported list: {field.FieldType.Name}");
                return;
            }

            IList currentList = GetManagedReferenceValue(inputProperty) as IList;
            string stateKey = GetManagedReferenceStateKey(inputProperty, field);
            if (DrawSupportedList(rect, currentList, field.FieldType, elementType, "Input", stateKey, false, out IList nextList))
            {
                SetManagedReferenceValue(inputProperty, nextList);
            }
        }

        private static float GetSupportedListInputHeight(SerializedProperty inputProperty, FieldInfo field)
        {
            float line = EditorGUIUtility.singleLineHeight;
            if (inputProperty == null
                || field == null
                || !FieldPatchUtility.TryGetSupportedListElementType(field.FieldType, out _))
            {
                return line;
            }

            return GetSupportedListHeight(
                GetManagedReferenceValue(inputProperty) as IList,
                GetManagedReferenceStateKey(inputProperty, field));
        }

        private static void DrawReadonlySupportedListValue(Rect rect, object currentValue, FieldInfo field, SerializedProperty inputProperty, string label)
        {
            if (field == null)
            {
                EditorGUI.LabelField(rect, label, "List field is missing");
                return;
            }

            if (!FieldPatchUtility.TryGetSupportedListElementType(field.FieldType, out Type elementType))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    DrawReadonlyValue(rect, field.FieldType, currentValue, label);
                }

                return;
            }

            DrawReadonlySupportedList(
                rect,
                currentValue as IList,
                field.FieldType,
                elementType,
                label,
                $"{GetManagedReferenceStateKey(inputProperty, field)}/readonly");
        }

        private static float GetReadonlySupportedListHeight(object currentValue, FieldInfo field, SerializedProperty inputProperty)
        {
            float line = EditorGUIUtility.singleLineHeight;
            if (field == null
                || !FieldPatchUtility.TryGetSupportedListElementType(field.FieldType, out _))
            {
                return line;
            }

            return GetSupportedListHeight(
                currentValue as IList,
                $"{GetManagedReferenceStateKey(inputProperty, field)}/readonly");
        }

        private static bool DrawSupportedListField(Rect rect, object owner, FieldInfo field, object currentValue, string stateKey, string label)
        {
            Type fieldType = field?.FieldType;
            if (fieldType == null || !FieldPatchUtility.TryGetSupportedListElementType(fieldType, out Type elementType))
            {
                return false;
            }

            if (!DrawSupportedList(rect, currentValue as IList, fieldType, elementType, label, stateKey, false, out IList nextList))
            {
                return false;
            }

            SetFieldValue(owner, field, nextList);
            return true;
        }

        private static void DrawReadonlySupportedList(Rect rect, IList currentList, Type listType, Type elementType, string label, string stateKey)
        {
            DrawSupportedList(rect, currentList, listType, elementType, label, stateKey, true, out _);
        }

        private static bool DrawSupportedList(Rect rect, IList currentList, Type listType, Type elementType, string label, string stateKey,
            bool isReadOnly, out IList nextList)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float addButtonWidth = 54f;
            const float removeButtonWidth = 24f;
            const float buttonSpacing = 4f;
            const float indentWidth = 15f;

            bool changed = false;
            IList list = currentList;
            nextList = list;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, line);
            Rect foldoutRect = headerRect;
            Rect addRect = default;

            if (!isReadOnly)
            {
                foldoutRect.width = Mathf.Max(0f, headerRect.width - addButtonWidth - buttonSpacing);
                addRect = new Rect(foldoutRect.xMax + buttonSpacing, headerRect.y, addButtonWidth, line);
            }

            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, $"{label} ({list?.Count ?? 0})", true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);

            if (!isReadOnly && GUI.Button(addRect, "Add"))
            {
                list ??= CreateManagedReferenceList(listType);
                if (list != null)
                {
                    list.Add(CreateDefaultSupportedListElement(elementType));
                    nextList = list;
                    changed = true;
                }
            }

            if (!nextExpanded)
            {
                return changed;
            }

            float y = headerRect.yMax + spacing;
            if (list == null)
            {
                EditorGUI.LabelField(
                    new Rect(rect.x + indentWidth, y, Mathf.Max(0f, rect.width - indentWidth), line),
                    "Null");
                return changed;
            }

            for (int i = 0; i < list.Count; i++)
            {
                string elementLabel = $"Element {i}";
                Rect elementRect = new Rect(rect.x + indentWidth, y, Mathf.Max(0f, rect.width - indentWidth), line);

                if (isReadOnly)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        DrawReadonlyValue(elementRect, elementType, list[i], elementLabel);
                    }
                }
                else
                {
                    Rect valueRect = new Rect(
                        elementRect.x,
                        elementRect.y,
                        Mathf.Max(0f, elementRect.width - removeButtonWidth - buttonSpacing),
                        line);
                    Rect removeRect = new Rect(valueRect.xMax + buttonSpacing, y, removeButtonWidth, line);

                    if (DrawEditableSupportedListElement(valueRect, elementLabel, elementType, list[i], out object nextElement))
                    {
                        list[i] = nextElement;
                        nextList = list;
                        changed = true;
                    }

                    if (GUI.Button(removeRect, "-"))
                    {
                        list.RemoveAt(i);
                        nextList = list;
                        return true;
                    }
                }

                y += line + spacing;
            }

            return changed;
        }

        private static float GetSupportedListHeight(IList list, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = line;

            if (!GetManagedReferenceFoldoutState(stateKey, true))
            {
                return height;
            }

            if (list == null)
            {
                return height + spacing + line;
            }

            for (int i = 0; i < list.Count; i++)
            {
                height += spacing + line;
            }

            return height;
        }

        private static bool DrawEditableSupportedListElement(Rect rect, string label, Type elementType, object currentValue, out object nextValue)
        {
            nextValue = currentValue;

            if (elementType == typeof(int))
            {
                int next = EditorGUI.DelayedIntField(rect, label, currentValue is int typed ? typed : default);
                if (!(currentValue is int currentInt) || currentInt != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(float))
            {
                float next = EditorGUI.DelayedFloatField(rect, label, currentValue is float typed ? typed : default);
                if (!(currentValue is float currentFloat) || !Mathf.Approximately(currentFloat, next))
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(double))
            {
                double next = EditorGUI.DoubleField(rect, label, currentValue is double typed ? typed : default);
                if (!(currentValue is double currentDouble) || Math.Abs(currentDouble - next) > double.Epsilon)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(long))
            {
                long next = EditorGUI.LongField(rect, label, currentValue is long typed ? typed : default);
                if (!(currentValue is long currentLong) || currentLong != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(bool))
            {
                bool next = EditorGUI.Toggle(rect, label, currentValue is bool typed && typed);
                if (!(currentValue is bool currentBool) || currentBool != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(string))
            {
                string next = EditorGUI.DelayedTextField(rect, label, currentValue as string ?? string.Empty);
                if (!string.Equals(currentValue as string, next, StringComparison.Ordinal))
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(Vector2))
            {
                Vector2 next = EditorGUI.Vector2Field(rect, label, currentValue is Vector2 typed ? typed : default);
                if (!(currentValue is Vector2 currentVector2) || currentVector2 != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(Vector3))
            {
                Vector3 next = EditorGUI.Vector3Field(rect, label, currentValue is Vector3 typed ? typed : default);
                if (!(currentValue is Vector3 currentVector3) || currentVector3 != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(Vector4))
            {
                Vector4 next = EditorGUI.Vector4Field(rect, label, currentValue is Vector4 typed ? typed : default);
                if (!(currentValue is Vector4 currentVector4) || currentVector4 != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(Color))
            {
                Color next = EditorGUI.ColorField(rect, label, currentValue is Color typed ? typed : Color.white);
                if (!(currentValue is Color currentColor) || currentColor != next)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType == typeof(LayerMask))
            {
                LayerMask next = DrawLayerMaskField(rect, label, currentValue is LayerMask typed ? typed : default);
                if (!(currentValue is LayerMask currentMask) || currentMask.value != next.value)
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType != null && elementType.IsEnum)
            {
                Array values = Enum.GetValues(elementType);
                Enum currentEnum = currentValue as Enum;
                if (currentEnum == null && values.Length > 0)
                {
                    currentEnum = (Enum)values.GetValue(0);
                }

                Enum next = currentEnum == null ? null : EditorGUI.EnumPopup(rect, label, currentEnum);
                if (!Equals(currentEnum, next))
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            if (elementType != null && typeof(UnityEngine.Object).IsAssignableFrom(elementType))
            {
                UnityEngine.Object next = EditorGUI.ObjectField(rect, label, currentValue as UnityEngine.Object, elementType, true);
                if (!ReferenceEquals(currentValue, next))
                {
                    nextValue = next;
                    return true;
                }

                return false;
            }

            EditorGUI.LabelField(rect, label, $"Unsupported ({elementType?.Name ?? "Unknown"})");
            return false;
        }

        private static object CloneSupportedListValue(object currentValue, Type listType)
        {
            if (!(currentValue is IList sourceList))
            {
                return null;
            }

            IList clonedList = CreateManagedReferenceList(listType);
            if (clonedList == null)
            {
                return null;
            }

            for (int i = 0; i < sourceList.Count; i++)
            {
                clonedList.Add(sourceList[i]);
            }

            return clonedList;
        }

        private static object CreateDefaultSupportedListElement(Type elementType)
        {
            if (elementType == null)
            {
                return null;
            }

            if (elementType == typeof(string))
            {
                return string.Empty;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(elementType))
            {
                return null;
            }

            if (elementType.IsEnum)
            {
                Array values = Enum.GetValues(elementType);
                return values.Length > 0 ? values.GetValue(0) : Activator.CreateInstance(elementType);
            }

            return elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
        }

        private static bool DrawManagedReferenceObjectInput(Rect rect, object currentValue, Type declaredType,
            SerializedProperty inputProperty, string stateKey, out object nextValue)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float typeButtonWidth = 140f;
            const float clearButtonWidth = 24f;
            const float buttonSpacing = 4f;
            const float indentWidth = 15f;

            nextValue = currentValue;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, line);
            float foldoutWidth = Mathf.Max(0f, rect.width - typeButtonWidth - clearButtonWidth - (buttonSpacing * 2f));
            Rect foldoutRect = new Rect(rect.x, rect.y, foldoutWidth, line);
            Rect typeRect = new Rect(foldoutRect.xMax + buttonSpacing, rect.y, typeButtonWidth, line);
            Rect clearRect = new Rect(typeRect.xMax + buttonSpacing, rect.y, clearButtonWidth, line);

            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, "Input", true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);

            if (GUI.Button(typeRect, GetManagedReferenceTypeButtonLabel(currentValue, declaredType)))
            {
                ShowManagedReferenceTypeMenu(typeRect, declaredType, true, selectedType =>
                {
                    SetManagedReferenceValue(inputProperty, CreateManagedReferenceInstance(selectedType));
                });
            }

            using (new EditorGUI.DisabledScope(currentValue == null))
            {
                if (GUI.Button(clearRect, "-"))
                {
                    SetManagedReferenceValue(inputProperty, null);
                    return false;
                }
            }

            if (!nextExpanded || currentValue == null)
            {
                return false;
            }

            float contentHeight = GetSerializedFieldsHeight(currentValue, stateKey);
            Rect contentRect = new Rect(
                rect.x + indentWidth,
                headerRect.yMax + spacing,
                Mathf.Max(0f, rect.width - indentWidth),
                contentHeight);

            return DrawSerializedFields(contentRect, currentValue, stateKey);
        }

        private static bool DrawManagedReferenceListInput(Rect rect, IList currentList, Type listType, Type elementType,
            SerializedProperty inputProperty, string stateKey, out object nextValue)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float addButtonWidth = 54f;
            const float typeButtonWidth = 140f;
            const float removeButtonWidth = 24f;
            const float buttonSpacing = 4f;
            const float indentWidth = 15f;

            bool changed = false;
            IList list = currentList;
            nextValue = list;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, line);
            Rect foldoutRect = new Rect(headerRect.x, headerRect.y, Mathf.Max(0f, headerRect.width - addButtonWidth - buttonSpacing), line);
            Rect addRect = new Rect(foldoutRect.xMax + buttonSpacing, headerRect.y, addButtonWidth, line);

            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, $"Input ({list?.Count ?? 0})", true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);
            if (GUI.Button(addRect, "Add"))
            {
                ShowManagedReferenceTypeMenu(addRect, elementType, false, selectedType =>
                {
                    IList editableList = GetManagedReferenceValue(inputProperty) as IList ?? CreateManagedReferenceList(listType);
                    if (editableList == null)
                    {
                        return;
                    }

                    editableList.Add(CreateManagedReferenceInstance(selectedType));
                    SetManagedReferenceValue(inputProperty, editableList);
                });
            }

            if (!nextExpanded)
            {
                return false;
            }

            float y = headerRect.yMax + spacing;
            if (list == null)
            {
                EditorGUI.LabelField(new Rect(rect.x + indentWidth, y, Mathf.Max(0f, rect.width - indentWidth), line), "Null");
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                object element = list[i];
                string elementKey = $"{stateKey}/Element[{i}]";
                Rect elementHeaderRect = new Rect(rect.x + indentWidth, y, Mathf.Max(0f, rect.width - indentWidth), line);
                float foldoutWidth = Mathf.Max(0f, elementHeaderRect.width - typeButtonWidth - removeButtonWidth - (buttonSpacing * 2f));
                Rect elementFoldoutRect = new Rect(elementHeaderRect.x, elementHeaderRect.y, foldoutWidth, line);
                Rect typeRect = new Rect(elementFoldoutRect.xMax + buttonSpacing, y, typeButtonWidth, line);
                Rect removeRect = new Rect(typeRect.xMax + buttonSpacing, y, removeButtonWidth, line);

                bool elementExpanded = GetManagedReferenceFoldoutState(elementKey, element != null);
                bool nextElementExpanded = EditorGUI.Foldout(elementFoldoutRect, elementExpanded, $"Element {i}", true);
                SetManagedReferenceFoldoutState(elementKey, nextElementExpanded);

                int elementIndex = i;
                if (GUI.Button(typeRect, GetManagedReferenceTypeButtonLabel(element, elementType)))
                {
                    ShowManagedReferenceTypeMenu(typeRect, elementType, true, selectedType =>
                    {
                        if (!(GetManagedReferenceValue(inputProperty) is IList editableList)
                            || elementIndex < 0
                            || elementIndex >= editableList.Count)
                        {
                            return;
                        }

                        editableList[elementIndex] = CreateManagedReferenceInstance(selectedType);
                        SetManagedReferenceValue(inputProperty, editableList);
                    });
                }

                if (GUI.Button(removeRect, "-"))
                {
                    if (!(GetManagedReferenceValue(inputProperty) is IList editableList)
                        || elementIndex < 0
                        || elementIndex >= editableList.Count)
                    {
                        continue;
                    }

                    editableList.RemoveAt(elementIndex);
                    SetManagedReferenceValue(inputProperty, editableList);
                    return false;
                }

                y += line + spacing;

                if (!nextElementExpanded)
                {
                    continue;
                }

                if (element == null)
                {
                    EditorGUI.LabelField(
                        new Rect(rect.x + (indentWidth * 2f), y, Mathf.Max(0f, rect.width - (indentWidth * 2f)), line),
                        "Null");
                    y += line + spacing;
                    continue;
                }

                float contentHeight = GetSerializedFieldsHeight(element, elementKey);
                Rect contentRect = new Rect(
                    rect.x + (indentWidth * 2f),
                    y,
                    Mathf.Max(0f, rect.width - (indentWidth * 2f)),
                    contentHeight);

                if (DrawSerializedFields(contentRect, element, elementKey))
                {
                    changed = true;
                }

                y += contentHeight + spacing;
            }

            nextValue = list;
            return changed;
        }

        private static float GetManagedReferenceListInputHeight(IList list, Type listType, Type elementType, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = line;

            if (!GetManagedReferenceFoldoutState(stateKey, true))
            {
                return height;
            }

            if (list == null)
            {
                return height + spacing + line;
            }

            for (int i = 0; i < list.Count; i++)
            {
                object element = list[i];
                string elementKey = $"{stateKey}/Element[{i}]";
                height += spacing + line;

                if (!GetManagedReferenceFoldoutState(elementKey, element != null))
                {
                    continue;
                }

                height += spacing + (element == null
                    ? line
                    : GetSerializedFieldsHeight(element, elementKey));
            }

            return height;
        }

        private static float GetManagedReferenceInputHeight(SerializedProperty inputProperty, FieldInfo field)
        {
            float line = EditorGUIUtility.singleLineHeight;
            if (inputProperty == null || field == null)
            {
                return line;
            }

            object currentValue = GetManagedReferenceValue(inputProperty);
            string stateKey = GetManagedReferenceStateKey(inputProperty, field);

            return TryGetManagedReferenceListElementType(field.FieldType, out Type elementType)
                ? GetManagedReferenceListInputHeight(currentValue as IList, field.FieldType, elementType, stateKey)
                : GetManagedReferenceObjectInputHeight(currentValue, stateKey);
        }

        private static float GetManagedReferenceObjectInputHeight(object currentValue, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = line;

            if (!GetManagedReferenceFoldoutState(stateKey, true) || currentValue == null)
            {
                return height;
            }

            height += spacing + GetSerializedFieldsHeight(currentValue, stateKey);
            return height;
        }

        private static void DrawReadonlyManagedReferenceValue(Rect rect, object currentValue, FieldInfo field, SerializedProperty inputProperty)
        {
            if (field == null)
            {
                EditorGUI.LabelField(rect, "Input", "Managed reference field is missing");
                return;
            }

            string stateKey = $"{GetManagedReferenceStateKey(inputProperty, field)}/readonly";
            if (TryGetManagedReferenceListElementType(field.FieldType, out Type elementType))
            {
                DrawReadonlyManagedReferenceList(rect, currentValue as IList, elementType, stateKey);
                return;
            }

            DrawReadonlyManagedReferenceObject(rect, currentValue, field.FieldType, stateKey);
        }

        private static float GetReadonlyManagedReferenceHeight(object currentValue, FieldInfo field, SerializedProperty inputProperty)
        {
            float line = EditorGUIUtility.singleLineHeight;
            if (field == null)
            {
                return line;
            }

            string stateKey = $"{GetManagedReferenceStateKey(inputProperty, field)}/readonly";
            return TryGetManagedReferenceListElementType(field.FieldType, out Type elementType)
                ? GetReadonlyManagedReferenceListHeight(currentValue as IList, elementType, stateKey)
                : GetReadonlyManagedReferenceObjectHeight(currentValue, stateKey);
        }

        private static void DrawReadonlyManagedReferenceObject(Rect rect, object currentValue, Type declaredType, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float typeLabelWidth = 140f;
            const float labelSpacing = 4f;
            const float indentWidth = 15f;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, line);
            float foldoutWidth = Mathf.Max(0f, rect.width - typeLabelWidth - labelSpacing);
            Rect foldoutRect = new Rect(rect.x, rect.y, foldoutWidth, line);
            Rect typeRect = new Rect(foldoutRect.xMax + labelSpacing, rect.y, typeLabelWidth, line);

            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, "Input", true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);
            EditorGUI.LabelField(typeRect, GetReadonlyManagedReferenceTypeLabel(currentValue, declaredType));

            if (!nextExpanded || currentValue == null)
            {
                return;
            }

            float contentHeight = GetReadonlySerializedFieldsHeight(currentValue, stateKey);
            Rect contentRect = new Rect(
                rect.x + indentWidth,
                headerRect.yMax + spacing,
                Mathf.Max(0f, rect.width - indentWidth),
                contentHeight);

            DrawReadonlySerializedFields(contentRect, currentValue, stateKey);
        }

        private static float GetReadonlyManagedReferenceObjectHeight(object currentValue, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = line;

            if (!GetManagedReferenceFoldoutState(stateKey, true) || currentValue == null)
            {
                return height;
            }

            height += spacing + GetReadonlySerializedFieldsHeight(currentValue, stateKey);
            return height;
        }

        private static void DrawReadonlyManagedReferenceList(Rect rect, IList list, Type elementType, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float typeLabelWidth = 140f;
            const float labelSpacing = 4f;
            const float indentWidth = 15f;

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, line);
            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(headerRect, isExpanded, $"Input ({list?.Count ?? 0})", true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);

            if (!nextExpanded)
            {
                return;
            }

            float y = headerRect.yMax + spacing;
            if (list == null)
            {
                EditorGUI.LabelField(new Rect(rect.x + indentWidth, y, Mathf.Max(0f, rect.width - indentWidth), line), "Null");
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                object element = list[i];
                string elementKey = $"{stateKey}/Element[{i}]";
                Rect elementHeaderRect = new Rect(rect.x + indentWidth, y, Mathf.Max(0f, rect.width - indentWidth), line);
                float foldoutWidth = Mathf.Max(0f, elementHeaderRect.width - typeLabelWidth - labelSpacing);
                Rect foldoutRect = new Rect(elementHeaderRect.x, elementHeaderRect.y, foldoutWidth, line);
                Rect typeRect = new Rect(foldoutRect.xMax + labelSpacing, y, typeLabelWidth, line);

                bool elementExpanded = GetManagedReferenceFoldoutState(elementKey, element != null);
                bool nextElementExpanded = EditorGUI.Foldout(foldoutRect, elementExpanded, $"Element {i}", true);
                SetManagedReferenceFoldoutState(elementKey, nextElementExpanded);
                EditorGUI.LabelField(typeRect, GetReadonlyManagedReferenceTypeLabel(element, elementType));

                y += line + spacing;

                if (!nextElementExpanded)
                {
                    continue;
                }

                if (element == null)
                {
                    EditorGUI.LabelField(
                        new Rect(rect.x + (indentWidth * 2f), y, Mathf.Max(0f, rect.width - (indentWidth * 2f)), line),
                        "Null");
                    y += line + spacing;
                    continue;
                }

                float contentHeight = GetReadonlySerializedFieldsHeight(element, elementKey);
                Rect contentRect = new Rect(
                    rect.x + (indentWidth * 2f),
                    y,
                    Mathf.Max(0f, rect.width - (indentWidth * 2f)),
                    contentHeight);

                DrawReadonlySerializedFields(contentRect, element, elementKey);
                y += contentHeight + spacing;
            }
        }

        private static float GetReadonlyManagedReferenceListHeight(IList list, Type elementType, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float height = line;

            if (!GetManagedReferenceFoldoutState(stateKey, true))
            {
                return height;
            }

            if (list == null)
            {
                return height + spacing + line;
            }

            for (int i = 0; i < list.Count; i++)
            {
                object element = list[i];
                string elementKey = $"{stateKey}/Element[{i}]";
                height += spacing + line;

                if (!GetManagedReferenceFoldoutState(elementKey, element != null))
                {
                    continue;
                }

                height += spacing + (element == null
                    ? line
                    : GetReadonlySerializedFieldsHeight(element, elementKey));
            }

            return height;
        }

        private static void DrawReadonlySerializedFields(Rect rect, object target, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(target?.GetType());

            if (target == null || fields.Count == 0)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.LabelField(rect, "No serialized fields");
                }

                return;
            }

            float y = rect.y;
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo childField = fields[i];
                object childValue = ReadFieldValue(target, childField);
                string childKey = $"{stateKey}/{FieldPatchUtility.GetFieldKey(childField)}";
                float childHeight = GetReadonlySerializedFieldHeight(childField, childValue, childKey);
                Rect childRect = new Rect(rect.x, y, rect.width, childHeight);

                DrawReadonlySerializedField(childRect, childField, childValue, childKey);
                y += childHeight;

                if (i < fields.Count - 1)
                {
                    y += spacing;
                }
            }
        }

        private static float GetReadonlySerializedFieldsHeight(object target, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(target?.GetType());

            if (target == null || fields.Count == 0)
            {
                return line;
            }

            float height = 0f;
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo childField = fields[i];
                object childValue = ReadFieldValue(target, childField);
                string childKey = $"{stateKey}/{FieldPatchUtility.GetFieldKey(childField)}";

                if (i > 0)
                {
                    height += spacing;
                }

                height += GetReadonlySerializedFieldHeight(childField, childValue, childKey);
            }

            return Mathf.Max(line, height);
        }

        private static void DrawReadonlySerializedField(Rect rect, FieldInfo field, object currentValue, string stateKey)
        {
            string label = ObjectNames.NicifyVariableName(FieldPatchUtility.GetDisplayName(field));
            if (FieldPatchUtility.TryGetSupportedListElementType(field?.FieldType, out Type elementType))
            {
                DrawReadonlySupportedList(rect, currentValue as IList, field.FieldType, elementType, label, stateKey);
                return;
            }

            if (CanDrawNestedSerializableField(field?.FieldType))
            {
                DrawReadonlyNestedSerializableField(rect, label, currentValue, stateKey);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                DrawReadonlyValue(rect, field?.FieldType, currentValue, label);
            }
        }

        private static float GetReadonlySerializedFieldHeight(FieldInfo field, object currentValue, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            if (FieldPatchUtility.TryGetSupportedListElementType(field?.FieldType, out _))
            {
                return GetSupportedListHeight(currentValue as IList, stateKey);
            }

            if (!CanDrawNestedSerializableField(field?.FieldType))
            {
                return line;
            }

            float height = line;
            if (!GetManagedReferenceFoldoutState(stateKey, true) || currentValue == null)
            {
                return height;
            }

            height += spacing + GetReadonlySerializedFieldsHeight(currentValue, stateKey);
            return height;
        }

        private static void DrawReadonlyNestedSerializableField(Rect rect, string label, object currentValue, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float indentWidth = 15f;

            Rect foldoutRect = new Rect(rect.x, rect.y, rect.width, line);
            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, label, true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);

            if (!nextExpanded || currentValue == null)
            {
                return;
            }

            float contentHeight = GetReadonlySerializedFieldsHeight(currentValue, stateKey);
            Rect contentRect = new Rect(
                rect.x + indentWidth,
                rect.y + line + spacing,
                Mathf.Max(0f, rect.width - indentWidth),
                contentHeight);

            DrawReadonlySerializedFields(contentRect, currentValue, stateKey);
        }

        private static string GetReadonlyManagedReferenceTypeLabel(object currentValue, Type declaredType)
        {
            return currentValue == null
                ? $"Null ({declaredType.Name})"
                : currentValue.GetType().Name;
        }

        private static string GetManagedReferenceStateKey(SerializedProperty inputProperty, FieldInfo field)
        {
            if (inputProperty == null || field == null)
            {
                return null;
            }

            return $"{inputProperty.serializedObject.targetObject.GetInstanceID()}::{inputProperty.propertyPath}::{FieldPatchUtility.GetFieldKey(field)}";
        }

        private static bool GetManagedReferenceFoldoutState(string key, bool defaultValue)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            if (!ManagedReferenceFoldoutStateByKey.TryGetValue(key, out bool isExpanded))
            {
                ManagedReferenceFoldoutStateByKey[key] = defaultValue;
                return defaultValue;
            }

            return isExpanded;
        }

        private static void SetManagedReferenceFoldoutState(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            ManagedReferenceFoldoutStateByKey[key] = value;
        }

        private static IList CreateManagedReferenceList(Type listType)
        {
            if (listType == null)
            {
                return null;
            }

            try
            {
                return Activator.CreateInstance(listType) as IList;
            }
            catch
            {
                return null;
            }
        }

        private static bool DrawSerializedFields(Rect rect, object target, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(target?.GetType());

            if (target == null || fields.Count == 0)
            {
                EditorGUI.LabelField(rect, "No serialized fields");
                return false;
            }

            float y = rect.y;
            bool changed = false;

            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo childField = fields[i];
                object childValue = ReadFieldValue(target, childField);
                string childKey = $"{stateKey}/{FieldPatchUtility.GetFieldKey(childField)}";
                float childHeight = GetSerializedFieldHeight(childField, childValue, childKey);
                Rect childRect = new Rect(rect.x, y, rect.width, childHeight);

                if (DrawSerializedField(childRect, target, childField, childValue, childKey))
                {
                    changed = true;
                }

                y += childHeight;
                if (i < fields.Count - 1)
                {
                    y += spacing;
                }
            }

            return changed;
        }

        private static float GetSerializedFieldsHeight(object target, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            IReadOnlyList<FieldInfo> fields = FieldPatchUtility.GetMutableFields(target?.GetType());

            if (target == null || fields.Count == 0)
            {
                return line;
            }

            float height = 0f;
            for (int i = 0; i < fields.Count; i++)
            {
                FieldInfo childField = fields[i];
                object childValue = ReadFieldValue(target, childField);
                string childKey = $"{stateKey}/{FieldPatchUtility.GetFieldKey(childField)}";

                if (i > 0)
                {
                    height += spacing;
                }

                height += GetSerializedFieldHeight(childField, childValue, childKey);
            }

            return Mathf.Max(line, height);
        }

        private static float GetSerializedFieldHeight(FieldInfo field, object currentValue, string stateKey)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (FieldPatchUtility.TryGetSupportedListElementType(field?.FieldType, out _))
            {
                return GetSupportedListHeight(currentValue as IList, stateKey);
            }

            if (field == null || !CanDrawNestedSerializableField(field.FieldType))
            {
                return line;
            }

            float height = line;
            if (!GetManagedReferenceFoldoutState(stateKey, true) || currentValue == null)
            {
                return height;
            }

            height += spacing + GetSerializedFieldsHeight(currentValue, stateKey);
            return height;
        }

        private static bool CanDrawNestedSerializableField(Type fieldType)
        {
            if (fieldType == null
                || fieldType.IsPrimitive
                || fieldType.IsEnum
                || fieldType.IsValueType
                || fieldType == typeof(decimal)
                || fieldType == typeof(string)
                || typeof(UnityEngine.Object).IsAssignableFrom(fieldType)
                || typeof(IList).IsAssignableFrom(fieldType))
            {
                return false;
            }

            return fieldType.IsDefined(typeof(SerializableAttribute), true);
        }

        private static bool DrawSerializedField(Rect rect, object owner, FieldInfo field, object currentValue, string stateKey)
        {
            Type fieldType = field?.FieldType;
            string label = ObjectNames.NicifyVariableName(FieldPatchUtility.GetDisplayName(field));

            if (FieldPatchUtility.TryGetSupportedListElementType(fieldType, out _))
            {
                return DrawSupportedListField(rect, owner, field, currentValue, stateKey, label);
            }

            if (fieldType == typeof(int))
            {
                int next = EditorGUI.DelayedIntField(rect, label, currentValue is int typed ? typed : default);
                if (!(currentValue is int currentInt) || currentInt != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(float))
            {
                float next = EditorGUI.DelayedFloatField(rect, label, currentValue is float typed ? typed : default);
                if (!(currentValue is float currentFloat) || !Mathf.Approximately(currentFloat, next))
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(double))
            {
                double next = EditorGUI.DoubleField(rect, label, currentValue is double typed ? typed : default);
                if (!(currentValue is double currentDouble) || Math.Abs(currentDouble - next) > double.Epsilon)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(long))
            {
                long next = EditorGUI.LongField(rect, label, currentValue is long typed ? typed : default);
                if (!(currentValue is long currentLong) || currentLong != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(bool))
            {
                bool next = EditorGUI.Toggle(rect, label, currentValue is bool typed && typed);
                if (!(currentValue is bool currentBool) || currentBool != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(string))
            {
                string next = EditorGUI.DelayedTextField(rect, label, currentValue as string ?? string.Empty);
                if (!string.Equals(currentValue as string, next, StringComparison.Ordinal))
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(Vector2))
            {
                Vector2 next = EditorGUI.Vector2Field(rect, label, currentValue is Vector2 typed ? typed : default);
                if (!(currentValue is Vector2 currentVector2) || currentVector2 != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(Vector3))
            {
                Vector3 next = EditorGUI.Vector3Field(rect, label, currentValue is Vector3 typed ? typed : default);
                if (!(currentValue is Vector3 currentVector3) || currentVector3 != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(Vector4))
            {
                Vector4 next = EditorGUI.Vector4Field(rect, label, currentValue is Vector4 typed ? typed : default);
                if (!(currentValue is Vector4 currentVector4) || currentVector4 != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(Color))
            {
                Color next = EditorGUI.ColorField(rect, label, currentValue is Color typed ? typed : Color.white);
                if (!(currentValue is Color currentColor) || currentColor != next)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType == typeof(LayerMask))
            {
                LayerMask next = DrawLayerMaskField(rect, label, currentValue is LayerMask typed ? typed : default);
                if (!(currentValue is LayerMask currentMask) || currentMask.value != next.value)
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType != null && fieldType.IsEnum)
            {
                Array values = Enum.GetValues(fieldType);
                Enum currentEnum = currentValue as Enum;
                if (currentEnum == null && values.Length > 0)
                {
                    currentEnum = (Enum)values.GetValue(0);
                }

                Enum next = currentEnum == null ? null : EditorGUI.EnumPopup(rect, label, currentEnum);
                if (!Equals(currentEnum, next))
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (fieldType != null && typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                UnityEngine.Object next = EditorGUI.ObjectField(rect, label, currentValue as UnityEngine.Object, fieldType, true);
                if (!ReferenceEquals(currentValue, next))
                {
                    SetFieldValue(owner, field, next);
                    return true;
                }

                return false;
            }

            if (CanDrawNestedSerializableField(fieldType))
            {
                return DrawNestedSerializableField(rect, owner, field, currentValue, stateKey, label);
            }

            EditorGUI.LabelField(rect, label, $"Unsupported ({fieldType?.Name ?? "Unknown"})");
            return false;
        }

        private static bool DrawNestedSerializableField(Rect rect, object owner, FieldInfo field, object currentValue, string stateKey, string label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            const float createButtonWidth = 54f;
            const float buttonSpacing = 4f;
            const float indentWidth = 15f;

            bool canCreate = currentValue == null && CanInstantiateManagedReferenceType(field?.FieldType);
            float foldoutWidth = canCreate
                ? Mathf.Max(0f, rect.width - createButtonWidth - buttonSpacing)
                : rect.width;

            Rect foldoutRect = new Rect(rect.x, rect.y, foldoutWidth, line);
            Rect createRect = new Rect(foldoutRect.xMax + buttonSpacing, rect.y, createButtonWidth, line);

            bool isExpanded = GetManagedReferenceFoldoutState(stateKey, true);
            bool nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, label, true);
            SetManagedReferenceFoldoutState(stateKey, nextExpanded);

            if (canCreate && GUI.Button(createRect, "New"))
            {
                SetFieldValue(owner, field, CreateManagedReferenceInstance(field.FieldType));
                return true;
            }

            if (!nextExpanded || currentValue == null)
            {
                return false;
            }

            float contentHeight = GetSerializedFieldsHeight(currentValue, stateKey);
            Rect contentRect = new Rect(
                rect.x + indentWidth,
                rect.y + line + spacing,
                Mathf.Max(0f, rect.width - indentWidth),
                contentHeight);

            return DrawSerializedFields(contentRect, currentValue, stateKey);
        }

        private static LayerMask DrawLayerMaskField(Rect rect, string label, LayerMask currentValue)
        {
            string[] layerNames = InternalEditorUtility.layers;
            int namedMask = 0;

            for (int i = 0; i < layerNames.Length; i++)
            {
                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0 && (currentValue.value & (1 << layer)) != 0)
                {
                    namedMask |= 1 << i;
                }
            }

            int nextNamedMask = EditorGUI.MaskField(rect, label, namedMask, layerNames);
            int nextValue = 0;

            for (int i = 0; i < layerNames.Length; i++)
            {
                if ((nextNamedMask & (1 << i)) == 0)
                {
                    continue;
                }

                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0)
                {
                    nextValue |= 1 << layer;
                }
            }

            currentValue.value = nextValue;
            return currentValue;
        }

        private static bool TryGetManagedReferenceListElementType(Type fieldType, out Type elementType)
        {
            elementType = null;
            if (fieldType == null)
            {
                return false;
            }

            if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(List<>))
            {
                return false;
            }

            Type candidateElementType = fieldType.GetGenericArguments()[0];
            if (candidateElementType == null
                || candidateElementType.IsValueType
                || candidateElementType == typeof(string)
                || typeof(UnityEngine.Object).IsAssignableFrom(candidateElementType))
            {
                return false;
            }

            elementType = candidateElementType;
            return true;
        }

        private static string GetManagedReferenceTypeButtonLabel(object currentValue, Type declaredType)
        {
            return currentValue == null ? $"Select ({declaredType.Name})" : currentValue.GetType().Name;
        }

        private static void ShowManagedReferenceTypeMenu(Rect buttonRect, Type declaredType, bool allowNull, Action<Type> onSelected)
        {
            GenericMenu menu = new GenericMenu();

            if (allowNull)
            {
                menu.AddItem(new GUIContent("Null"), false, () => onSelected?.Invoke(null));
                menu.AddSeparator(string.Empty);
            }

            List<Type> candidateTypes = GetManagedReferenceCandidateTypes(declaredType);
            if (candidateTypes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent($"No Types ({declaredType.Name})"));
            }
            else
            {
                for (int i = 0; i < candidateTypes.Count; i++)
                {
                    Type candidateType = candidateTypes[i];
                    string path = GetManagedReferenceTypeMenuPath(candidateType);
                    menu.AddItem(new GUIContent(path), false, () => onSelected?.Invoke(candidateType));
                }
            }

            menu.DropDown(buttonRect);
        }

        private static List<Type> GetManagedReferenceCandidateTypes(Type declaredType)
        {
            List<Type> candidates = new List<Type>();
            HashSet<Type> seen = new HashSet<Type>();

            if (CanInstantiateManagedReferenceType(declaredType))
            {
                candidates.Add(declaredType);
                seen.Add(declaredType);
            }

            foreach (Type candidateType in TypeCache.GetTypesDerivedFrom(declaredType))
            {
                if (!CanInstantiateManagedReferenceType(candidateType) || !seen.Add(candidateType))
                {
                    continue;
                }

                candidates.Add(candidateType);
            }

            candidates.Sort((left, right) => string.CompareOrdinal(GetManagedReferenceTypeMenuPath(left), GetManagedReferenceTypeMenuPath(right)));
            return candidates;
        }

        private static bool CanInstantiateManagedReferenceType(Type type)
        {
            if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            {
                return false;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                return false;
            }

            return true;
        }

        private static string GetManagedReferenceTypeMenuPath(Type type)
        {
            if (type == null)
            {
                return "Null";
            }

            return string.IsNullOrEmpty(type.Namespace)
                ? type.Name
                : $"{type.Namespace.Replace('.', '/')}/{type.Name}";
        }

        private static object CreateManagedReferenceInstance(Type type)
        {
            if (type == null)
            {
                return null;
            }

            try
            {
                return Activator.CreateInstance(type, true);
            }
            catch
            {
                try
                {
                    return FormatterServices.GetUninitializedObject(type);
                }
                catch
                {
                    return null;
                }
            }
        }

        private static object CloneManagedReferenceValue(object value, UnityEngine.Object targetObject, FieldInfo field)
        {
            if (value == null)
            {
                return null;
            }

            if (targetObject == null || field == null)
            {
                return value;
            }

            UnityEngine.Object source = CreateEditableTargetClone(targetObject);
            UnityEngine.Object destination = CreateEditableTargetClone(targetObject);
            if (source == null || destination == null)
            {
                DestroyEditableTargetClone(source);
                DestroyEditableTargetClone(destination);
                return value;
            }

            try
            {
                SetFieldValue(source, field, value);

                string json = EditorJsonUtility.ToJson(source);
                EditorJsonUtility.FromJsonOverwrite(json, destination);
                return ReadFieldValue(destination, field);
            }
            finally
            {
                DestroyEditableTargetClone(source);
                DestroyEditableTargetClone(destination);
            }
        }

        private static UnityEngine.Object CreateEditableTargetClone(UnityEngine.Object targetObject)
        {
            if (targetObject == null)
            {
                return null;
            }

            if (targetObject is Component component)
            {
                GameObject cloneGameObject = UnityEngine.Object.Instantiate(component.gameObject);
                cloneGameObject.hideFlags = HideFlags.HideAndDontSave;

                Component[] originalComponents = component.gameObject.GetComponents(component.GetType());
                Component[] clonedComponents = cloneGameObject.GetComponents(component.GetType());
                int componentIndex = Array.IndexOf(originalComponents, component);

                if (componentIndex >= 0 && componentIndex < clonedComponents.Length)
                {
                    return clonedComponents[componentIndex];
                }

                return clonedComponents.Length > 0 ? clonedComponents[0] : cloneGameObject;
            }

            UnityEngine.Object clone = UnityEngine.Object.Instantiate(targetObject);
            if (clone != null)
            {
                clone.hideFlags = HideFlags.HideAndDontSave;
            }

            return clone;
        }

        private static void DestroyEditableTargetClone(UnityEngine.Object cloneTarget)
        {
            if (cloneTarget == null)
            {
                return;
            }

            if (cloneTarget is Component component)
            {
                UnityEngine.Object.DestroyImmediate(component.gameObject);
                return;
            }

            UnityEngine.Object.DestroyImmediate(cloneTarget);
        }

        private static void SetFieldValue(object targetObject, FieldInfo field, object value)
        {
            if (targetObject == null || field == null)
            {
                return;
            }

            try
            {
                field.SetValue(targetObject, value);
            }
            catch
            {
                // ignore drawing failures and fall back to read-only messages
            }
        }

        private static object GetManagedReferenceValue(SerializedProperty inputProperty)
        {
            FieldPatchValue inputValue = SerializedPropertyRuntime.GetValue(inputProperty) as FieldPatchValue;
            return inputValue?.GetManagedReferenceValue();
        }

        private static void SetManagedReferenceValue(SerializedProperty inputProperty, object value)
        {
            FieldPatchValue inputValue = SerializedPropertyRuntime.GetValue(inputProperty) as FieldPatchValue;
            if (inputValue == null)
            {
                return;
            }

            UnityEngine.Object hostObject = inputProperty.serializedObject.targetObject;
            Undo.RecordObject(hostObject, "Edit Field Patch Managed Reference");
            inputValue.SetManagedReference(value);
            EditorUtility.SetDirty(hostObject);
            InternalEditorUtility.RepaintAllViews();
        }

        private static void DrawHelpBox(ref Rect row, string message, MessageType messageType)
        {
            float height = GetHelpBoxHeight();
            Rect helpRect = new Rect(row.x, row.y, row.width, height);
            EditorGUI.HelpBox(helpRect, message, messageType);
            row.y += height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static float GetHelpBoxHeight()
        {
            return EditorGUIUtility.singleLineHeight * HelpBoxLines;
        }

        private static object ReadFieldValue(object target, FieldInfo field)
        {
            if (target == null || field == null)
            {
                return null;
            }

            try
            {
                return field.GetValue(target);
            }
            catch
            {
                return null;
            }
        }

        private static void TryAutoSync(SerializedProperty property, IFieldPatchRuntime binder)
        {
            SerializedProperty inputsProperty = property.FindPropertyRelative("_inputs");
            if (inputsProperty == null)
            {
                return;
            }

            IReadOnlyList<FieldInfo> fields = binder.GetMutableFields();

            if (inputsProperty.arraySize != fields.Count)
            {
                SyncBinder(property, "Auto Sync Patch Inputs");
                return;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                string key = FieldPatchUtility.GetFieldKey(fields[i]);
                if (FindInputByKey(inputsProperty, key) == null)
                {
                    SyncBinder(property, "Auto Sync Patch Inputs");
                    return;
                }
            }
        }

        private static SerializedProperty FindInputByKey(SerializedProperty inputsProperty, string fieldKey)
        {
            if (inputsProperty == null)
            {
                return null;
            }

            for (int i = 0; i < inputsProperty.arraySize; i++)
            {
                SerializedProperty element = inputsProperty.GetArrayElementAtIndex(i);
                SerializedProperty keyProperty = element.FindPropertyRelative("_fieldKey");

                if (keyProperty != null && keyProperty.stringValue == fieldKey)
                {
                    return element;
                }
            }

            return null;
        }

        private static void SyncBinder(SerializedProperty property, string undoName)
        {
            IFieldPatchRuntime binder = SerializedPropertyRuntime.GetValue(property) as IFieldPatchRuntime;
            if (binder == null)
            {
                return;
            }

            UnityEngine.Object host = property.serializedObject.targetObject;
            Undo.RecordObject(host, undoName);
            binder.SyncInputs();
            EditorUtility.SetDirty(host);
        }

        private static string GetMessageKey(SerializedProperty property)
        {
            return $"{property.serializedObject.targetObject.GetInstanceID()}::{property.propertyPath}";
        }

        private static void SetMessage(SerializedProperty property, string message)
        {
            MessageByKey[GetMessageKey(property)] = message;
        }

        private static bool TryGetMessage(SerializedProperty property, out string message)
        {
            return MessageByKey.TryGetValue(GetMessageKey(property), out message);
        }
    }

    internal static class SerializedPropertyRuntime
    {
        public static object GetValue(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }

            object currentObject = property.serializedObject.targetObject;
            string path = property.propertyPath.Replace(".Array.data[", "[");
            string[] elements = path.Split('.');

            for (int i = 0; i < elements.Length; i++)
            {
                string element = elements[i];
                int indexStart = element.IndexOf('[');

                if (indexStart >= 0)
                {
                    string memberName = element.Substring(0, indexStart);
                    int index = Convert.ToInt32(element.Substring(indexStart + 1, element.Length - indexStart - 2));
                    currentObject = GetIndexedMemberValue(currentObject, memberName, index);
                }
                else
                {
                    currentObject = GetMemberValue(currentObject, element);
                }

                if (currentObject == null)
                {
                    return null;
                }
            }

            return currentObject;
        }

        private static object GetMemberValue(object source, string memberName)
        {
            if (source == null)
            {
                return null;
            }

            Type sourceType = source.GetType();

            while (sourceType != null)
            {
                FieldInfo field = sourceType.GetField(memberName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                {
                    return field.GetValue(source);
                }

                PropertyInfo property = sourceType.GetProperty(memberName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (property != null)
                {
                    return property.GetValue(source);
                }

                sourceType = sourceType.BaseType;
            }

            return null;
        }

        private static object GetIndexedMemberValue(object source, string memberName, int index)
        {
            object enumerableObject = GetMemberValue(source, memberName);
            if (enumerableObject == null)
            {
                return null;
            }

            if (enumerableObject is IList list)
            {
                return index >= 0 && index < list.Count ? list[index] : null;
            }

            if (enumerableObject is IEnumerable enumerable)
            {
                IEnumerator enumerator = enumerable.GetEnumerator();
                for (int i = 0; i <= index; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        return null;
                    }
                }

                return enumerator.Current;
            }

            return null;
        }
    }
}




