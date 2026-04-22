using System;
using DewmoLib.ObjectPool.Editor;
using DewmoLib.ObjectPool.RunTime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Work.LKW.Code.Items.ItemInfo;

namespace Code.DataSystem.Editor
{
    public class ItemSOPreview
    {
        private Label _nameLabel;
        private Label _idLabel;
        private VisualElement _rootElement;

        public event Action<ItemSOPreview> OnSelectEvent;
        
        public string Name
        {
            get => _nameLabel.text;
            set => _nameLabel.text = value;
        }

        public string Id
        {
            get => _idLabel.text;
            set => _idLabel.text = value;
        }
        [FormerlySerializedAs("poolItem")] public ItemDataSO itemData;

        public bool IsActive
        {
            get => _rootElement.ClassListContains("active");
            set => _rootElement.EnableInClassList("active", value);
        }
        public ItemSOPreview(VisualElement root, ItemDataSO itemData)
        {
            this.itemData = itemData;
            _rootElement = root.Q<VisualElement>("ItemSOPreview");
            _nameLabel = root.Q<Label>("ItemName");
            _idLabel = root.Q<Label>("ItemID");
            
            _rootElement.RegisterCallback<ClickEvent>(evt =>
            {
                OnSelectEvent?.Invoke(this);
                evt.StopPropagation();
            });
        }
    }
}