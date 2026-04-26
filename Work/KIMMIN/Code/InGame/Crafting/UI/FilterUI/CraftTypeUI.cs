using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftTypeUI : MonoBehaviour
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private Sprite sprite;
        [SerializeField] private string itemName;
        [SerializeField] private Color backgroundColor;
        
        [Header("UI Elements")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private Image background;
        [SerializeField] private Button selectButton;
            
        public Action<ItemType> OnItemSelected;

        private void Awake()
        {
            selectButton.onClick.AddListener(() => OnItemSelected?.Invoke(itemType));
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
    }
}