using Unity.Properties;
using UnityEngine.UIElements;

namespace Code.DataSystem.Editor
{
    public class PropertyLabel
    {
        private VisualElement _rootElement;
        private TextField _textField;

        public PropertyLabel(VisualElement root)
        {
            _rootElement = root;
            _textField = _rootElement.Q<TextField>("PropertyField");
        }

        public void SetProperty(string name, string value)
        {
            _textField.label = name;
            _textField.value = value;
        }
        
    }
}