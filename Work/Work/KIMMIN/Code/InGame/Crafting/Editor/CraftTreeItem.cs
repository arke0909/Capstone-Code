using System;
using UnityEngine.UIElements;
using Work.Code.Crafting;

namespace Code.InGame.Crafting.Editor
{
    public class CraftTreeItem
    {
        private Label _nameLabel;
        private Button _deleteBtn;
        private VisualElement _rootElement;
        
        public event Action<CraftTreeSO> OnDeleteEvent;
        public event Action<CraftTreeSO> OnSelectEvent;
        
        public string Name
        {
            get => _nameLabel.text;
            set => _nameLabel.text = value;
        }
        
        public CraftTreeItem(VisualElement root, CraftTreeSO data)
        {
            _rootElement = root.Q<VisualElement>("PoolItem");
            _nameLabel = root.Q<Label>("ItemName");
            _deleteBtn = root.Q<Button>("DeleteBtn");
            _deleteBtn.RegisterCallback<ClickEvent>(evt =>
            {
                OnDeleteEvent?.Invoke(data);
                evt.StopPropagation();
            });
            
            _rootElement.RegisterCallback<ClickEvent>(evt =>
            {
                OnSelectEvent?.Invoke(data);
                evt.StopPropagation();
            });
        }
    }
}