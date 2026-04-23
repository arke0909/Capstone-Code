using System;
using UnityEngine;
using UnityEngine.UIElements;
using Work.Code.Crafting;

namespace Code.InGame.Crafting.Editor
{
    public class CraftNode
    {
        private Label _label;
        private VisualElement _icon;
        
        public Button NodeButton { get; set; }
        public int Index { get; set; }
        public NodeData NodeData { get; set; }
        public event Action<CraftNode> OnSelectEvent;

        public string Name
        {
            get => _label.text;
            set => _label.text = value;
        }

        public Sprite Icon
        {
            get => _icon.style.backgroundImage.value.sprite;
            set
            {
                if (value != null)
                    _icon.style.backgroundImage = Background.FromSprite(value);
                else
                    _icon.style.backgroundImage = null;
            }
        }

        public StyleColor BGColor
        {
            get => NodeButton.style.backgroundColor;
            set => NodeButton.style.backgroundColor = value;
        }

        public CraftNode(VisualElement root, NodeData data)
        {
            NodeData = data;
            NodeButton = root.Q<Button>("NodeButton");
            _label = root.Q<Label>("ItemName");
            _icon = root.Q<VisualElement>("Icon");
            
            NodeButton.RegisterCallback<ClickEvent>(evt =>
            {
                OnSelectEvent?.Invoke(this);
            });
        }
    }
}