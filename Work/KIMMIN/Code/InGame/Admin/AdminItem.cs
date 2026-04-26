using Code.Players;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Work.LKW.Code.Items;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Admin
{
    public class AdminItem : MonoBehaviour, IUIElement<ItemDataSO>
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI itemText;
        [SerializeField] private Button button;

        private ItemDataSO _item;
        private PlayerInventory _inventory;

        private void Start()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            ItemBase item = _item.CreateItem().Item;
            _inventory.TryAddItem(item, item.ItemData.maxStack);
        }

        public void SetInventory(PlayerInventory inventory)
        {
            _inventory = inventory;
        }

        public void EnableFor(ItemDataSO item)
        {
            image.sprite = item.itemImage;
            itemText.text = item.itemName;
            _item = item;
        }

        public void ClearUI() { }
    }
}