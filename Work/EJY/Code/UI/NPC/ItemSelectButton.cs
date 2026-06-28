using System;
using Code.Items.ItemInfo;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.UI.NPC
{
    public class ItemSelectButton : MonoBehaviour, IDisposable
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private Image outlineImage;
        [SerializeField] private Button selectBtn;
        [SerializeField] private Color selectedOutlineColor = new(255, 220, 90, 255);

        private UnityAction _selectAction;
        private Color _defaultOutlineColor;

        protected ItemDataSO _itemData;

        public ItemDataSO ItemData => _itemData;
        public GameObject GameObject => gameObject;

        protected void InitItem(ItemDataSO itemData)
        {
            if (itemData == null)
                throw new ArgumentNullException(nameof(itemData));

            _itemData = itemData;
            int rarityIndex = (int)_itemData.rarity;
            if ((uint)rarityIndex >= UIDefine.RarityColors.Length)
                throw new InvalidOperationException($"Unsupported rarity {_itemData.rarity} on {_itemData.itemName}.");

            background.gameObject.SetActive(true);
            icon.gameObject.SetActive(true);
            icon.enabled = true;
            nameText.transform.parent.gameObject.SetActive(true);
            nameText.gameObject.SetActive(true);

            background.color = UIDefine.RarityColors[rarityIndex];
            icon.sprite = itemData.itemImage;
            nameText.text = itemData.itemName;
            SetSelectedState(false);
            GameObject.SetActive(true);
        }

        protected void BindSelect(Action<ItemDataSO> onSelect)
        {
            if (_selectAction != null)
                selectBtn.onClick.RemoveListener(_selectAction);

            _selectAction = () => onSelect(_itemData);
            selectBtn.onClick.AddListener(_selectAction);
        }

        public void SetSelectedState(bool isSelected)
        {
            outlineImage.gameObject.SetActive(true);
            outlineImage.color = isSelected ? selectedOutlineColor : _defaultOutlineColor;
        }

        public void Hide()
        {
            GameObject.SetActive(false);
        }

        public virtual void Dispose()
        {
            if (_selectAction != null)
            {
                selectBtn.onClick.RemoveListener(_selectAction);
                _selectAction = null;
            }
        }

        private void Awake()
        {
            _defaultOutlineColor = outlineImage.color;
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
