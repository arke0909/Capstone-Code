using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftTypeUI : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemType[] additionalItemTypes;
        [SerializeField] private Sprite sprite;
        [SerializeField] private string itemName;
        [SerializeField] private Color backgroundColor;
        
        [Header("UI Elements")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private Image background;
        [SerializeField] private Button selectButton;

        private ItemType[] _itemTypes;

        public ItemType[] ItemTypes => _itemTypes ??= GetItemTypes();
        public string ItemName => itemName;
        public Action<CraftTypeUI> OnItemSelected;

        private void Awake()
        {
            _itemTypes = GetItemTypes();
            selectButton.onClick.AddListener(() => OnItemSelected?.Invoke(this));
        }

        private void OnDestroy()
        {
            selectButton.onClick.RemoveAllListeners();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(icon != null)
                icon.sprite = sprite;
            if(typeText != null)
                typeText.text = itemName;
            if(background != null)
                background.color = backgroundColor;
            
            name = $"{itemType.ToString()}_제작버튼";
        }
#endif

        private ItemType[] GetItemTypes()
        {
            int length = additionalItemTypes == null ? 1 : additionalItemTypes.Length + 1;
            ItemType[] itemTypes = new ItemType[length];

            itemTypes[0] = itemType;

            for (int i = 1; i < length; i++)
            {
                itemTypes[i] = additionalItemTypes[i - 1];
            }

            return itemTypes;
        }
    }
}
