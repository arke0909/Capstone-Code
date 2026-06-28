using System.Collections.Generic;
using System.Linq;
using Chipmunk.ComponentContainers;
using Code.Items;
using Code.Items.ItemInfo;
using Code.Players;
using Scripts.Players;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.NPC
{
    public class AntiqueShopView : NPCInteractUIContent
    {
        [SerializeField] private int submitForExchangeCnt = 5;
        [SerializeField] private Button exchangeBtn;
        [SerializeField] private TextMeshProUGUI exchangeCountText;
        [SerializeField] private Transform anyHasItemTextTrm;
        [SerializeField] private Transform targetItemGridTrm;
        [SerializeField] private Transform submitItemGridTrm;
        [SerializeField] private TargetItemSelectButton targetItemButtonPrefab;
        [SerializeField] private SubmitItemSelectButton submitItemButtonPrefab;
        [SerializeField] private ItemDataBaseSO itemDB;

        private readonly List<TargetItemSelectButton> _targetItemSelectButtons = new();
        private readonly List<SubmitItemSelectButton> _submitItemSelectButtons = new();
        private AntiqueShopController _antiqueShopController;
        private Player _player;

        public override void Init(Player player)
        {
            base.Init(player);
            _player = player;

            exchangeBtn.onClick.AddListener(HandleExchangeBtnClick);
        }

        public override void EnableUI(bool isFade = false)
        {
            if (_antiqueShopController == null)
            {
                PlayerInventory playerInventory = _player.Get<PlayerInventory>();
                if (playerInventory == null)
                    throw new MissingReferenceException(
                        $"{nameof(AntiqueShopView)} requires {nameof(PlayerInventory)}.");

                _antiqueShopController = new AntiqueShopController(playerInventory, itemDB, submitForExchangeCnt);
            }

            _antiqueShopController.PlayerInventory.InventoryChanged -= HandleInventoryChanged;
            _antiqueShopController.PlayerInventory.InventoryChanged += HandleInventoryChanged;
            _antiqueShopController.SyncSelectedSubmitItems();

            RefreshTargetButtons();
            RefreshSubmitButtons();
            UpdateExchangeState();
            base.EnableUI(isFade);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            exchangeBtn.onClick.RemoveListener(HandleExchangeBtnClick);

            if (_antiqueShopController != null)
                _antiqueShopController.PlayerInventory.InventoryChanged -= HandleInventoryChanged;

            foreach (var targetButton in _targetItemSelectButtons)
            {
                targetButton.Dispose();
            }

            foreach (var submitButton in _submitItemSelectButtons)
            {
                submitButton.Dispose();
            }
        }

        private void HandleExchangeBtnClick()
        {
            if (_antiqueShopController.CanExchange() == false)
                return;

            foreach (var pair in _antiqueShopController.SelectedSubmitItems.ToList())
            {
                if (_antiqueShopController.PlayerInventory.RemoveItemByData(pair.Key, pair.Value) == false)
                    throw new UnityException($"Failed to remove submit item : {pair.Key.itemName}");
            }

            if (_antiqueShopController.PlayerInventory.TryAddItem(_antiqueShopController.TargetItemData.CreateItem()
                    .Item) == false)
                throw new UnityException(
                    $"Failed to add target item : {_antiqueShopController.TargetItemData.itemName}");

            _antiqueShopController.ClearSelectedSubmitItems();
            RefreshSubmitButtons();
            UpdateExchangeState();
        }

        private void HandleSubmitItemSelected(ItemDataSO itemData)
        {
            if (_antiqueShopController.TrySelectSubmitItem(itemData) == false)
                return;

            UpdateSubmitButtonStates();
            UpdateExchangeState();
        }

        private void HandleSubmitItemDeselected(ItemDataSO itemData)
        {
            if (_antiqueShopController.TryDeselectSubmitItem(itemData) == false)
                return;

            UpdateSubmitButtonStates();
            UpdateExchangeState();
        }

        private void HandleTargetItemSelected(ItemDataSO itemData)
        {
            _antiqueShopController.SelectTargetItem(itemData);

            foreach (var targetButton in _targetItemSelectButtons)
            {
                if (targetButton.GameObject.activeSelf == false)
                    continue;

                targetButton.SetSelectedState(targetButton.ItemData == itemData);
            }

            UpdateExchangeState();
        }

        private void RefreshTargetButtons()
        {
            IReadOnlyList<ItemDataSO> targetItems = _antiqueShopController.TargetItems;
            EnsureTargetButtonCount(targetItems.Count);

            foreach (var targetButton in _targetItemSelectButtons)
            {
                targetButton.Hide();
            }

            for (int i = 0; i < targetItems.Count; i++)
            {
                _targetItemSelectButtons[i].Init(targetItems[i], HandleTargetItemSelected);
                _targetItemSelectButtons[i].SetSelectedState(targetItems[i] == _antiqueShopController.TargetItemData);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)targetItemGridTrm);
        }

        private void RefreshSubmitButtons()
        {
            List<MaterialItem> submitItems = _antiqueShopController.GetSubmitItems();

            anyHasItemTextTrm.gameObject.SetActive(submitItems.Count == 0);

            EnsureSubmitButtonCount(submitItems.Count);

            foreach (var submitButton in _submitItemSelectButtons)
            {
                submitButton.Hide();
            }

            for (int i = 0; i < submitItems.Count; i++)
            {
                ItemDataSO itemData = submitItems[i].ItemData;
                int selectedCount = _antiqueShopController.GetSelectedCount(itemData);

                _submitItemSelectButtons[i].Init(itemData, _antiqueShopController.GetOwnedCount(itemData),
                    selectedCount,
                    HandleSubmitItemSelected, HandleSubmitItemDeselected);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)submitItemGridTrm);
        }

        private void HandleInventoryChanged()
        {
            _antiqueShopController.SyncSelectedSubmitItems();
            RefreshSubmitButtons();
            UpdateExchangeState();
        }

        private void UpdateSubmitButtonStates()
        {
            foreach (var submitButton in _submitItemSelectButtons)
            {
                if (submitButton.GameObject.activeSelf == false)
                    continue;

                int selectedCount = _antiqueShopController.GetSelectedCount(submitButton.ItemData);
                submitButton.SetOwnedCount(_antiqueShopController.GetOwnedCount(submitButton.ItemData));
                submitButton.SetSelectedState(selectedCount > 0);
                submitButton.SetSubmitCount(selectedCount);
            }
        }

        private void UpdateExchangeState()
        {
            int selectedSubmitItemCount = _antiqueShopController.GetCurrentSubmitCount();
            exchangeCountText.text = $"{selectedSubmitItemCount} / {_antiqueShopController.RequiredSubmitCount}";
            exchangeBtn.interactable = _antiqueShopController.CanExchange();
        }

        private void EnsureTargetButtonCount(int requiredCount)
        {
            while (_targetItemSelectButtons.Count < requiredCount)
            {
                TargetItemSelectButton button = Instantiate(targetItemButtonPrefab, targetItemGridTrm);
                button.gameObject.SetActive(true);
                _targetItemSelectButtons.Add(button);
            }
        }

        private void EnsureSubmitButtonCount(int requiredCount)
        {
            while (_submitItemSelectButtons.Count < requiredCount)
            {
                SubmitItemSelectButton button = Instantiate(submitItemButtonPrefab, submitItemGridTrm);
                button.gameObject.SetActive(true);
                _submitItemSelectButtons.Add(button);
            }
        }
    }
}