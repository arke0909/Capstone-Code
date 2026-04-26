using Code.Players;
using Code.UI.Core;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Craft
{
    public class CraftPinItemUI : LayoutUIBase
    {
        [SerializeField] private ItemDataUI targetItem;
        [SerializeField] private Transform itemRoot;

        private ItemDataUI[] _itemUIs;
        private CraftTreeSO _craftTree;
        private PlayerInventory _inventory;

        public void Init(PlayerInventory inventory)
        {
            _inventory = inventory;
            
            _itemUIs = itemRoot.GetComponentsInChildren<ItemDataUI>(true);
            _inventory.InventoryChanged += HandleChangeInventory;
            ClearUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _inventory.InventoryChanged -= HandleChangeInventory;
        }

        private void HandleChangeInventory()
        {
            if (_craftTree != null)
                RefreshUI();
        }

        public void EnablePin(CraftTreeSO craftItem)
        {
            _craftTree = craftItem;
            targetItem.EnableDataUI(craftItem.Item);
            
            EnableUI();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if(_craftTree.ConsumeItems.Count == 0)
                return;
            
            int cnt = 0;
            foreach (var item in _craftTree.ConsumeItems)
            {
                _itemUIs[cnt].EnableDataUI(item.Key);
                _itemUIs[cnt].SetNameColor(SetItemColor(item.Key, item.Value));
                _itemUIs[cnt].SetCountText($"[{_inventory.GetItemCount(item.Key)}/{item.Value}]");
                cnt++;
            }
        }
        
        private Color32 SetItemColor(ItemDataSO item, int count)
        {
            return _inventory.GetItemCount(item) >= count ?
                UIDefine.GreenColor : UIDefine.RedColor;
        }

        public void ClearUI()
        {
            foreach (ItemDataUI ui in _itemUIs)
            {
                ui.DisableUI();
            }
            
            targetItem.DisableUI();
            DisableUI();
        }
    }
}