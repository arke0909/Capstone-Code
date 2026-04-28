using System;
using UnityEngine.UIElements;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.DataSystem.Editor
{
    public class TypeSelectButton
    {
        public Action<ItemType> OnButtonClicked;
        private VisualElement _rootElement;
        
        public Button Button { get; private set; }
        public ItemType Type { get; private set; }
        
        public string Name
        {
            get => Button.text;
            set => Button.text = value;
        }

        public TypeSelectButton(VisualElement root, string name, ItemType type)
        {
            Type= type;
            _rootElement = root;
            Button = root.Q<Button>("SelectBtn");
            Name = name;

            Button.clicked += () => OnButtonClicked?.Invoke(Type);
        }
    }
}