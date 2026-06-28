using System;
using System.Collections.Generic;
using System.Linq;
using Code.Items;
using Code.Items.ItemInfo;
using Code.Players;
using UnityEngine;

namespace Code.UI.NPC
{
    public class AntiqueShopController
    {
        private readonly Dictionary<ItemDataSO, int> _selectedSubmitItems = new();

        public AntiqueShopController(PlayerInventory playerInventory, ItemDataBaseSO itemDB, int submitForExchangeCnt)
        {
            PlayerInventory = playerInventory ??
                              throw new MissingReferenceException($"{nameof(AntiqueShopController)} requires {nameof(PlayerInventory)}.");
            if (itemDB == null)
                throw new ArgumentNullException(nameof(itemDB));
            if (submitForExchangeCnt <= 0)
                throw new ArgumentOutOfRangeException(nameof(submitForExchangeCnt));

            RequiredSubmitCount = submitForExchangeCnt;
            TargetItems = itemDB.GetItemsByType(ItemType.Material)
                .Where(item => item.rarity == Rarity.Common)
                .ToList();
        }

        public PlayerInventory PlayerInventory { get; }
        public IReadOnlyDictionary<ItemDataSO, int> SelectedSubmitItems => _selectedSubmitItems;
        public IReadOnlyList<ItemDataSO> TargetItems { get; }
        public ItemDataSO TargetItemData { get; private set; }
        public int RequiredSubmitCount { get; }

        public void SelectTargetItem(ItemDataSO itemData)
        {
            TargetItemData = itemData;
        }

        public bool TrySelectSubmitItem(ItemDataSO itemData)
        {
            if (itemData == null)
                throw new ArgumentNullException(nameof(itemData));

            if (GetCurrentSubmitCount() == RequiredSubmitCount)
                return false;

            int currentSelectedCount = GetSelectedCount(itemData);
            if (currentSelectedCount == PlayerInventory.GetItemCount(itemData))
                return false;

            _selectedSubmitItems[itemData] = currentSelectedCount + 1;
            return true;
        }

        public bool TryDeselectSubmitItem(ItemDataSO itemData)
        {
            if (itemData == null)
                throw new ArgumentNullException(nameof(itemData));

            int currentSelectedCount = GetSelectedCount(itemData);
            if (currentSelectedCount == 0)
                return false;

            if (currentSelectedCount == 1)
                _selectedSubmitItems.Remove(itemData);
            else
                _selectedSubmitItems[itemData] = currentSelectedCount - 1;

            return true;
        }

        public void SyncSelectedSubmitItems()
        {
            foreach (ItemDataSO itemData in _selectedSubmitItems.Keys.ToList())
            {
                int remainCount = PlayerInventory.GetItemCount(itemData);
                if (remainCount == 0)
                {
                    _selectedSubmitItems.Remove(itemData);
                    continue;
                }

                if (_selectedSubmitItems[itemData] > remainCount)
                    _selectedSubmitItems[itemData] = remainCount;
            }
        }

        public List<MaterialItem> GetSubmitItems()
        {
            return PlayerInventory.GetItems<MaterialItem>();
        }

        public int GetOwnedCount(ItemDataSO itemData)
        {
            return PlayerInventory.GetItemCount(itemData);
        }

        public int GetSelectedCount(ItemDataSO itemData)
        {
            if (_selectedSubmitItems.TryGetValue(itemData, out int count))
                return count;

            return 0;
        }

        public int GetCurrentSubmitCount()
        {
            return _selectedSubmitItems.Values.Sum();
        }

        public bool CanExchange()
        {
            return TargetItemData != null && GetCurrentSubmitCount() == RequiredSubmitCount;
        }

        public void ClearSelectedSubmitItems()
        {
            _selectedSubmitItems.Clear();
        }
    }
}
