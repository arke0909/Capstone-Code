using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Scripts.SkillSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Code.SkillSystem.Upgrade.Editor
{
    [CustomEditor(typeof(SkillUpgradeSO))]
    public class SkillUpgradeSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset view = default;

        private SkillUpgradeSO _targetSO;
        private VisualElement _root;
        private VisualElement _fieldInfoBox;
        private VisualElement _methodInfoBox;

        public override VisualElement CreateInspectorGUI()
        {
            _targetSO = target as SkillUpgradeSO;
            _root = new VisualElement();

            InspectorElement.FillDefaultInspector(_root, serializedObject, this);

            if (view != null) view.CloneTree(_root);

            _fieldInfoBox = _root.Q<VisualElement>("FieldInfoBox");
            _methodInfoBox = _root.Q<VisualElement>("MethodInfoBox");

            _root.Q<Button>("ValidateBtn").clicked += () =>
            {
                EditorUtility.DisplayDialog("Validate", $"{_targetSO.InitializeUpgrade()}", "OK");
            };

            InitSkillDropDown();
            RegisterChangeCallbacks();

            UpdateReflection();

            return _root;
        }

        private void InitSkillDropDown()
        {
            DropdownField skillDropdown = _root.Q<DropdownField>("TargetSkillDropdown");

            Type skillType = typeof(Skill);
            List<string> derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assem => assem.GetTypes())
                .Where(type => type.IsSubclassOf(skillType) && !type.IsAbstract)
                .Select(type => type.AssemblyQualifiedName)
                .ToList();

            skillDropdown.choices = derivedTypes;

            skillDropdown.RegisterValueChangedCallback(evt =>
            {
                UpdateProperty("targetSkillName", evt.newValue);
                UpdateReflection();
            });

            skillDropdown.value = _targetSO.targetSkillName;
        }

        private void RegisterChangeCallbacks()
        {
            _root.TrackSerializedObjectValue(serializedObject, (so) => { UpdateReflection(); });
        }

        private void UpdateProperty(string propertyName, string value)
        {
            var prop = serializedObject.FindProperty(propertyName);
            if (prop != null && prop.stringValue != value)
            {
                prop.stringValue = value;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_targetSO);

                EditorApplication.delayCall += () =>
                {
                    string result = _targetSO.InitializeUpgrade();
                    Debug.Log($"Recompiled Upgrade: {result}");
                    UpdateReflection();
                };
            }
        }

        private void ResetFieldData()
        {
            var nameProp = serializedObject.FindProperty("fieldName");
            if (nameProp != null) nameProp.stringValue = string.Empty;

            var floatProp = serializedObject.FindProperty("floatValue");
            if (floatProp != null) floatProp.floatValue = 0f;

            var intProp = serializedObject.FindProperty("intValue");
            if (intProp != null) intProp.intValue = 0;

            serializedObject.ApplyModifiedProperties();

            var fieldDropdown = _root.Q<DropdownField>("FieldListDropdown");
            if (fieldDropdown != null) fieldDropdown.value = string.Empty;
        }

        private void UpdateReflection()
        {
            if (string.IsNullOrEmpty(_targetSO.targetSkillName)) return;

            if (_targetSO.UpgradeType == UpgradeType.FieldUpdate)
            {
                _fieldInfoBox.style.display = DisplayStyle.Flex;
                _methodInfoBox.style.display = DisplayStyle.None;
                UpdateFieldChoices();
            }
            else
            {
                _fieldInfoBox.style.display = DisplayStyle.None;
                _methodInfoBox.style.display = DisplayStyle.Flex;
                UpdateMethodChoices();
            }
        }

        private void UpdateFieldChoices()
        {
            DropdownField fieldDropdown = _root.Q<DropdownField>("FieldListDropdown");
            Type skillType = Type.GetType(_targetSO.targetSkillName);
            if (skillType == null) return;

            Type targetType = _targetSO.fieldType switch
            {
                FieldType.Float => typeof(float),
                FieldType.Int32 => typeof(int),
                FieldType.Boolean => typeof(bool),
                _ => typeof(float)
            };

            var validFields = skillType.GetFields(_targetSO.bindingFlags)
                .Where(f => f.FieldType == targetType)
                .Select(f => f.Name).ToList();

            fieldDropdown.choices = validFields;
            fieldDropdown.RegisterValueChangedCallback(evt => UpdateProperty("fieldName", evt.newValue));

            _root.Q<FloatField>("FloatValue").style.display =
                (_targetSO.fieldType == FieldType.Float) ? DisplayStyle.Flex : DisplayStyle.None;
            _root.Q<IntegerField>("IntegerValue").style.display =
                (_targetSO.fieldType == FieldType.Int32) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void UpdateMethodChoices()
        {
            DropdownField upgradeDropdown = _root.Q<DropdownField>("UpgradeMethodNameDropdown");
            DropdownField rollbackDropdown = _root.Q<DropdownField>("RollbackMethodNameDropdown");

            Type skillType = Type.GetType(_targetSO.targetSkillName);

            MethodInfo[] methodInfos = skillType.GetMethods(_targetSO.bindingFlags);

            upgradeDropdown.choices = methodInfos
                .Where(method => method.ReturnType == typeof(void))
                .Select(method => method.Name).ToList();

            rollbackDropdown.choices = methodInfos
                .Where(method => method.ReturnType == typeof(void))
                .Select(method => method.Name).ToList();

            if (upgradeDropdown.choices.Contains(_targetSO.upgradeMethodName) == false)
            {
                _targetSO.upgradeMethodName =
                    upgradeDropdown.choices.Count > 0 ? upgradeDropdown.choices.First() : string.Empty;
                EditorUtility.SetDirty(_targetSO);
            }

            if (rollbackDropdown.choices.Contains(_targetSO.rollbackMethodName) == false)
            {
                _targetSO.rollbackMethodName = rollbackDropdown.choices.Count > 0
                    ? rollbackDropdown.choices.First()
                    : string.Empty;
                EditorUtility.SetDirty(_targetSO);
            }
        }
    }
}