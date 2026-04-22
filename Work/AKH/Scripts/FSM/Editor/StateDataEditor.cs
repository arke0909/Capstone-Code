using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

//닷트윈 
namespace Scripts.FSM.Editor
{
    [UnityEditor.CustomEditor(typeof(StateDataSO))]
    public class StateDataEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset uiAsset = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            uiAsset.CloneTree(root);
            
            DropdownField classField = root.Q<DropdownField>("ClassDropdownField");
            
            classField.choices.Clear();
            var stateTypes = GetStateFromAssembly(typeof(State));
            classField.choices.AddRange(stateTypes);
            return root;
        }

        private static List<string> GetStateFromAssembly(Type targetType)
        {
            Assembly fsmAssembly = Assembly.GetAssembly(targetType);
            List<string> stateTypes = fsmAssembly.GetTypes()
                .Where(type => type.IsAbstract == false 
                               && type.IsSubclassOf(typeof(State)))
                .Select(type => type.FullName)
                .ToList();
            return stateTypes;
        }
    }
}