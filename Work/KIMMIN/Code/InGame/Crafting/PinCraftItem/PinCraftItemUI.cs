using System;
using Code.Players;
using Code.UI.Core;
using TMPro;
using UnityEngine;
using Work.LKW.Code.Items.ItemInfo;

namespace Work.Code.Crafting
{
    public class PinCraftItemUI : LayoutUIBase, IUIElement<CraftTreeSO>
    {
        [SerializeField] private ItemDataUI targetItem;
        [SerializeField] private Transform itemRoot;

        private ItemDataUI[] items;
        private CraftTreeSO _tree;
        private PlayerInventory _inventory;

        public void Init(PlayerInventory inventory)
        {
            _inventory = inventory;
            items = itemRoot.GetComponentsInChildren<ItemDataUI>(true);
            _inventory.InventoryChanged += HandleChagneInventory;
            Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _inventory.InventoryChanged -= HandleChagneInventory;
        }

        private void HandleChagneInventory()
        {
            if (_tree == null) return;
            RefreshUI();
        }

        public void EnableFor(CraftTreeSO craftItem)
        {
            _tree = craftItem;
            targetItem.EnableFor(craftItem.Item);
            EnableUI();
            RefreshUI();
        }

        private void RefreshUI()
        {
            int cnt = 0;
            if(_tree.CosumeItems.Count == 0) return;
            foreach (var item in _tree.CosumeItems)
            {
                items[cnt].EnableFor(item.Key);
                items[cnt].SetNameColor(SetItemColor(item.Key, item.Value));
                items[cnt].SetCountText($"[{_inventory.GetItemCount(item.Key)}/{item.Value}]");
                cnt++;
            }
        }
        
        private Color32 SetItemColor(ItemDataSO item, int count)
        {
            return _inventory.GetItemCount(item) >= count ?
                UIDefine.GreenColor : UIDefine.RedColor;
        }

        public void Clear()
        {
            foreach (var ui in items)
            {
                ui.DisableUI();
            }
            
            targetItem.DisableUI();
            DisableUI();
        }
    }
}